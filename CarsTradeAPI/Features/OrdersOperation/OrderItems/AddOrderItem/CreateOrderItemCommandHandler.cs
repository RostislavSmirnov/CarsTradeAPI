using MediatR;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Entities;
using AutoMapper;
using CarsTradeAPI.Features.OrdersOperation.OrdersRepository;
using CarsTradeAPI.Infrastructure.Services.CarInventoryService;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;


namespace CarsTradeAPI.Features.OrdersOperation.OrderItems.AddOrderItem
{
    public class CreateOrderItemCommandHandler : IRequestHandler<CreateOrderItemCommand, MbResult<ShowOrderDto>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<CreateOrderItemCommandHandler> _logger;
        private readonly ICarInventoryService _carInventoryService;
        private readonly IIdempotencyService _idempotencyService;
        public CreateOrderItemCommandHandler(IMapper mapper,
            IOrderRepository orderRepository,
            ICarInventoryService carInventoryService,
            ILogger<CreateOrderItemCommandHandler> logger,
            IIdempotencyService idempotencyService)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _carInventoryService = carInventoryService;
            _logger = logger;
            _idempotencyService = idempotencyService;
        }


        public async Task<MbResult<ShowOrderDto>> Handle(CreateOrderItemCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды CreateOrderItemCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                Order? existingOrder = await _orderRepository.GetOrderByIdAsync(resourceId.Value);
                ShowOrderDto dto = _mapper.Map<ShowOrderDto>(existingOrder);
                return MbResult<ShowOrderDto>.Success(dto);
            }

            // Проверка существования заказа
            Order? order = await _orderRepository.GetOrderByIdAsync(request.OrderId);
            if (order == null)
            {
                _logger.LogWarning("Попытка создать элемент заказа, в заказе с ID: {OrderId} которого не существует", request.OrderId);
                return MbResult<ShowOrderDto>.Failure(new[]
                {
                    new ErrorDetail("NOT_FOUND", $"Попытка создать элемент заказа, в заказе с ID: {request.OrderId} которого не существует", "CreateOrderItemCommand")
                });
            }

            // Проверка существования модели автомобиля
            CarInventoryServiceResult availabilityResult = await _carInventoryService.CheckAvailabilityAsync(request.CarModelId, (uint)request.OrderQuantity);
            if (availabilityResult.IsAvailable != true || availabilityResult.AvailableQuantity < request.OrderQuantity)
            {
                _logger.LogWarning("Попытка добавить автоимобиль c ID {CarModelId}) которого нет в нужном количестве {OrderQuantity}", request.CarModelId, request.OrderQuantity);
                return MbResult<ShowOrderDto>.Failure(new[]
                {
                    new ErrorDetail("NOT_FOUND, OrderQuantity",$"Попытка добавить автоимобиль c ID {request.CarModelId}) которого нет в нужном количестве {request.OrderQuantity}","CreateOrderItemCommand")
                });
            }
            
            try
            {
                // Маппинг запроса в сущность OrderItem
                OrderItem newOrderItem = _mapper.Map<OrderItem>(request);
                newOrderItem.UnitPrice = await _orderRepository.GetCarPrice(request.CarModelId);
                newOrderItem.OrderId = request.OrderId;

                // Уменьшение количества авто в инвентаре
                await _carInventoryService.DecreaseInventoryAsync(request.CarModelId, (uint)request.OrderQuantity);

                // Добавление нового элемента заказа
                await _orderRepository.AddOrderItem(order.OrderId, newOrderItem);

                // Маппинг обновленного заказа в DTO для ответа
                ShowOrderDto result = _mapper.Map<ShowOrderDto>(order);

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "OrderId", order.OrderId, json);

                _logger.LogInformation("Элемент заказа в заказ с ID: {OrderId} успешно добавлен", request.OrderId);
                return MbResult<ShowOrderDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке добавления элемента заказа в заказ с ID {OrderId}", request.OrderId);
                return MbResult<ShowOrderDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "AddOrderItemCommand")
                });
            }
        }
    }
}
