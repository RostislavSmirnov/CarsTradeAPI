using MediatR;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Entities;
using AutoMapper;
using CarsTradeAPI.Features.OrdersOperation.OrdersRepository;
using CarsTradeAPI.Infrastructure.Services.CarInventoryService;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;


namespace CarsTradeAPI.Features.OrdersOperation.OrderItems.AddOrderItems
{
    public class AddOrderItemsCommandHandler : IRequestHandler<AddOrderItemsCommand, MbResult<ShowOrderDto>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<AddOrderItemsCommandHandler> _logger;
        private readonly ICarInventoryService _carInventoryService;
        private readonly IIdempotencyService _idempotencyService;
        public AddOrderItemsCommandHandler(
            IMapper mapper,
            IOrderRepository orderRepository,
            ICarInventoryService carInventoryService,
            ILogger<AddOrderItemsCommandHandler> logger,
            IIdempotencyService idempotencyService)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _carInventoryService = carInventoryService;
            _logger = logger;
            _idempotencyService = idempotencyService;
        }


        public async Task<MbResult<ShowOrderDto>> Handle(AddOrderItemsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки запроса AddOrderItemsCommand");

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
                _logger.LogWarning("Попытка добавить элементы заказа, в заказ с ID: {OrderId} которого не существует", request.OrderId);
                return MbResult<ShowOrderDto>.Failure(new[]
                {
                    new ErrorDetail("NOT_FOUND",$"Попытка добавить элементы заказа, в заказ с ID: {request.OrderId} которого не существует","AddOrderItemsCommand")
                });
            }

            try
            {
                // Маппинг запроса в сущность OrderItem
                List<OrderItem> newOrderItems = _mapper.Map<List<OrderItem>>(request.OrderItems);

                // Проверка и обработка каждого элемента заказа
                foreach (OrderItem item in newOrderItems)
                {
                    // Проверка существования модели автомобиля
                    bool carModelExists = await _orderRepository.CheckCarModelAsync(item.CarModelId);
                    if (carModelExists != true)
                    {
                        _logger.LogWarning("Попытка добавить несуществующую модель автомобиля с ID: {CarModelId}",item.CarModelId);
                        return MbResult<ShowOrderDto>.Failure(
                            [
                                new ErrorDetail("NOT_FOUND", $"Попытка добавить несуществующую модель автомобиля с ID: {item.CarModelId}", "AddOrderItemsCommand")
                            ]);
                    }

                    // Проверка доступности модели автомобиля в инвентаре
                    CarInventoryServiceResult availabilityResult = await _carInventoryService.CheckAvailabilityAsync(item.CarModelId, (uint)item.OrderQuantity);
                    if (availabilityResult.IsAvailable != true || availabilityResult.AvailableQuantity < item.OrderQuantity)
                    {
                        //throw new Exception($"Модель автомобиля с ID {item.CarModelId} недоступна в запрашиваемом количестве {item.OrderQuantity}. В наличии {availabilityResult.AvailableQuantity}");
                        _logger.LogWarning("Модель автомобиля с ID {CarModelId} недоступна в запрашиваемом количестве {OrderQuantity}. В наличии {AvailableQuantity}", item.CarModelId, item.OrderQuantity, availabilityResult.AvailableQuantity);
                        return MbResult<ShowOrderDto>.Failure(
                            [
                                new ErrorDetail("NOT_FOUND", $"Модель автомобиля с ID {item.CarModelId} недоступна в запрашиваемом количестве {item.OrderQuantity}. В наличии {availabilityResult.AvailableQuantity}","AddOrderItemsCommand")
                            ]);
                    }

                    // Уменьшение количества авто в инвентаре
                    await _carInventoryService.DecreaseInventoryAsync(item.CarModelId, (uint)item.OrderQuantity);

                    // Установка цены, ID заказа, и добавление элемента в заказ
                    item.UnitPrice = await _orderRepository.GetCarPrice(item.CarModelId);
                    item.OrderId = request.OrderId;
                    order.OrderItems!.Add(item);
                }

                // Сохранение изменений в заказе
                await _orderRepository.AddOrderItems(order);

                // Маппинг обновленного заказа в DTO для ответа
                ShowOrderDto result = _mapper.Map<ShowOrderDto>(order);

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "OrderId", order.OrderId, json);

                _logger.LogInformation("Элементы заказа в заказ с ID: {OrderId} успешно добавлены", request.OrderId);
                return MbResult<ShowOrderDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке добавления элементов заказа в заказ с ID {OrderId}", request.OrderId);
                return MbResult<ShowOrderDto>.Failure(
                    [
                        new ErrorDetail("Exception",$"Ошибка при попытке добавления элементов заказа в заказ с ID {request.OrderId},{ex.Message}","AddOrderItemsCommand")
                    ]);
            }
        }
    }
}
