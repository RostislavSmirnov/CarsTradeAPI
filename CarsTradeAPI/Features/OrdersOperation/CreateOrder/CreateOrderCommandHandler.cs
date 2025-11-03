using MediatR;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Entities;
using AutoMapper;
using CarsTradeAPI.Features.OrdersOperation.OrdersRepository;
using CarsTradeAPI.Infrastructure.Services.CarInventoryService;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;
using CarsTradeAPI.Infrastructure.Services.CacheService;
using Contracts.Events;
using MassTransit;


namespace CarsTradeAPI.Features.OrdersOperation.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, MbResult<ShowOrderDto>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<CreateOrderCommandHandler> _logger;
        private readonly ICarInventoryService _carInventoryService;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ICacheService _cache;
        private readonly IPublishEndpoint _publishEndpoint;
        public CreateOrderCommandHandler(
            IMapper mapper,
            IOrderRepository orderRepository,
            ILogger<CreateOrderCommandHandler> logger,
            ICarInventoryService carInventoryService,
            IIdempotencyService idempotencyService,
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _logger = logger;
            _carInventoryService = carInventoryService;
            _idempotencyService = idempotencyService;
            _cache = cacheService;
            _publishEndpoint = publishEndpoint;
        }


        public async Task<MbResult<ShowOrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды CreateOrderCommand для ключа {IdempotencyKey}", request.IdempotencyKey);

            // 1 Проверка идемпотентности
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                Order? existingOrder = await _orderRepository.GetOrderByIdAsync(resourceId.Value);
                ShowOrderDto dto = _mapper.Map<ShowOrderDto>(existingOrder);
                return MbResult<ShowOrderDto>.Success(dto);
            }

            // 2 Проверка существования Покупателя
            bool buyerExists = await _orderRepository.CheckOrderBuyerAsync(request.BuyerId);
            if (buyerExists == false)
            {
                _logger.LogWarning("Покупатель с ID {BuyerId} не найден", request.BuyerId);
                return MbResult<ShowOrderDto>.Failure(new[] 
                { 
                    new ErrorDetail("NOT_FOUND", $"Покупатель с ID {request.BuyerId} не найден", "BuyerId") 
                });
            }

            // 3 Проверка существования Сотрудника
            bool employeeExists = await _orderRepository.CheckOrderEmployeeAsync(request.EmployeeId);
            if (employeeExists == false)
            {
                _logger.LogWarning("Сотрудник с ID {EmployeeId} не найден", request.EmployeeId);
                return MbResult<ShowOrderDto>.Failure(new[] 
                { 
                    new ErrorDetail("NOT_FOUND", $"Сотрудник с ID {request.EmployeeId} не найден", "EmployeeId") 
                });
            }

            try
            {
                // 4 Основная логика
                Order newOrder = _mapper.Map<Order>(request);
                newOrder.OrderId = Guid.NewGuid();
                newOrder.OrderCreateDate = DateTime.UtcNow;

                if (newOrder.OrderItems != null && newOrder.OrderItems.Any())
                {
                    foreach (OrderItem item in newOrder.OrderItems)
                    {
                        bool carModelExists = await _orderRepository.CheckCarModelAsync(item.CarModelId);
                        if (!carModelExists)
                        {
                            return MbResult<ShowOrderDto>.Failure(new[] { new ErrorDetail("NOT_FOUND", $"Модель автомобиля с ID {item.CarModelId} не найдена.", "OrderItems.CarModelId") });
                        }

                        CarInventoryServiceResult availabilityResult = await _carInventoryService.CheckAvailabilityAsync(item.CarModelId, (uint)item.OrderQuantity);
                        if (!availabilityResult.IsAvailable || availabilityResult.AvailableQuantity < item.OrderQuantity)
                        {
                            return MbResult<ShowOrderDto>.Failure(new[] { new ErrorDetail("VALIDATION_ERROR", $"Недостаточное количество на складе для модели с ID {item.CarModelId}. Запрошено: {item.OrderQuantity}, Доступно: {availabilityResult.AvailableQuantity}", "OrderItems.OrderQuantity") });
                        }

                        item.OrderId = newOrder.OrderId;
                        item.OrderItemId = Guid.NewGuid();
                        item.UnitPrice = await _orderRepository.GetCarPrice(item.CarModelId);
                    }

                    // Уменьшаем инвентарь только после всех проверок
                    foreach (var item in newOrder.OrderItems)
                    {
                        await _carInventoryService.DecreaseInventoryAsync(item.CarModelId, (uint)item.OrderQuantity);
                    }
                }

                newOrder.OrderPrice = newOrder.OrderItems?.Sum(item => item.UnitPrice * item.OrderQuantity) ?? 0;

                Order createdOrder = await _orderRepository.CreateOrderAsync(newOrder);
                ShowOrderDto result = _mapper.Map<ShowOrderDto>(createdOrder);

                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "Order", createdOrder.OrderId, json);

                await _cache.RemoveAsync("Order:all");

                // публикация события RabbitMq
                OrderCreated evt = new OrderCreated(result.OrderId, result.BuyerId, result.OrderPrice, result.OrderCreateDate);
                await _publishEndpoint.Publish(evt);

                _logger.LogInformation("Заказ успешно создан с ID: {OrderId}", newOrder.OrderId);
                return MbResult<ShowOrderDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке создать заказ");
                return MbResult<ShowOrderDto>.Failure(new[] 
                { 
                    new ErrorDetail("Exception", ex.Message, "CreateOrderCommand") 
                });
            }
        }
    }
}