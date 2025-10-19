using MediatR;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using AutoMapper;
using CarsTradeAPI.Features.OrdersOperation.OrdersRepository;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Infrastructure.Services.CarInventoryService;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;


namespace CarsTradeAPI.Features.OrdersOperation.OrderItems.DeleteOrderItems
{
    public class DeleteOrderItemsCommandHandler : IRequestHandler<DeleteOrderItemsCommand, MbResult<ShowOrderDto>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly ICarInventoryService _carInventoryService;
        private readonly ILogger<DeleteOrderItemsCommandHandler> _logger;
        private readonly IIdempotencyService _idempotencyService;
        public DeleteOrderItemsCommandHandler(
            IMapper mapper,
            IOrderRepository orderRepository,
            ICarInventoryService carInventoryService,
            ILogger<DeleteOrderItemsCommandHandler> logger,
            IIdempotencyService idempotencyService)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _carInventoryService = carInventoryService;
            _logger = logger;
            _idempotencyService = idempotencyService;
        }


        public async Task<MbResult<ShowOrderDto>> Handle(DeleteOrderItemsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки запроса DeleteOrderItemsCommand");

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
                _logger.LogWarning("Попытка удалить элементы заказа из несуществующего заказа с ID: {OrderId}", request.OrderId);
                return MbResult<ShowOrderDto>.Failure(
                    [
                        new ErrorDetail("NOT_FOUND",$"Попытка удалить элементы заказа из несуществующего заказа с ID: {request.OrderId}","DeleteOrderItemsCommand")
                    ]);
            }

            try
            {
                // Количество удаленных элементов
                int deletedItemsCount = 0;

                // Создание HashSet для быстрого поиска ID элементов для удаления
                HashSet<Guid> guidsToRemove = request.OrderItemId.ToHashSet();
                
                // Поиск элементов заказа, которые нужно удалить
                List<OrderItem> itemsToRemove = order.OrderItems!.Where(item => guidsToRemove.Contains(item.OrderItemId)).ToList();

                // Удаление найденных элементов из заказа
                foreach (OrderItem item in itemsToRemove)
                {
                    order.OrderItems.Remove(item);
                    deletedItemsCount++;

                    // Восстановление количества авто в инвентаре
                    await _carInventoryService.IncreaseInventoryAsync(item.CarModelId, (uint)item.OrderQuantity);
                }

                // Проверка, были ли удалены элементы
                if (deletedItemsCount == 0)
                {
                    _logger.LogWarning("Элементы заказа с такими ID не найдены");
                    return MbResult<ShowOrderDto>.Failure(
                        [
                            new ErrorDetail("NOT_FOUND","Элементы заказа с такими ID не найдены", "DeleteOrderItemsCommand")
                        ]);
                }

                // Обновление общей цены заказа
                order.OrderPrice = order.OrderItems?.Sum(item => item.UnitPrice * item.OrderQuantity) ?? 0;

                // Сохранение изменений в заказе, возврат обновленного заказа в DTO
                Order editedOrder = await _orderRepository.EditOrderAsync(order);
                ShowOrderDto result = _mapper.Map<ShowOrderDto>(editedOrder);

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "OrderId", order.OrderId, json);

                _logger.LogInformation("Элементы заказа из заказа с ID: {OrderId} успешно удалены", request.OrderId);
                return MbResult<ShowOrderDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке удалить элемениы заказа из заказа с ID: {OrderId}", request.OrderId);
                return MbResult<ShowOrderDto>.Failure(
                    [
                        new ErrorDetail("Exception",$"{ex.Message}","DeleteOrderItemsCommand")
                    ]);
            }
        }
    }
}
