using MediatR;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Entities;
using AutoMapper;
using CarsTradeAPI.Features.OrdersOperation.OrdersRepository;
using CarsTradeAPI.Infrastructure.Services.CarInventoryService;
using Microsoft.EntityFrameworkCore;
using CarsTradeAPI.Data;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage;


namespace CarsTradeAPI.Features.OrdersOperation.OrderItems.EditOrderItem
{
    public class EditOrderItemCommandHandler : IRequestHandler<EditOrderItemCommand, MbResult<ShowOrderDto>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly ICarInventoryService _carInventoryService;
        private readonly CarsTradeDbContext _dbContext;
        private readonly ILogger<EditOrderItemCommandHandler> _logger;
        private readonly IIdempotencyService _idempotencyService;
        public EditOrderItemCommandHandler(
            IMapper mapper,
            IOrderRepository orderRepository,
            ICarInventoryService carInventoryService,
            CarsTradeDbContext dbContext,
            ILogger<EditOrderItemCommandHandler> logger,
            IIdempotencyService idempotencyService)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _carInventoryService = carInventoryService;
            _dbContext = dbContext;
            _logger = logger;
            _idempotencyService = idempotencyService;
        }


        public async Task<MbResult<ShowOrderDto>> Handle(EditOrderItemCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды EditOrderItemCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                Order? existingOrder = await _orderRepository.GetOrderByIdAsync(resourceId.Value);
                ShowOrderDto dto = _mapper.Map<ShowOrderDto>(existingOrder);
                return MbResult<ShowOrderDto>.Success(dto);
            }

            // Загрузка заказа, и проверка его существованмя
            Order? order = await _orderRepository.GetOrderByIdAsync(request.OrderId);
            if (order == null)
            {
                _logger.LogWarning("Попытка изменить несуществующий заказ с ID: {OrderId}", request.OrderId);
                return MbResult<ShowOrderDto>.Failure(
                    [
                        new ErrorDetail("MOT_FOUND",$"Попытка изменить несуществующий заказ с ID: {request.OrderId}", "EditOrderItemCommand")
                    ]);
            }
                
            // Поиск элемента заказа в заказе
            OrderItem? item = order.OrderItems!.FirstOrDefault(oi => oi.OrderItemId == request.OrderItemId);
            if (item == null)
            {
                _logger.LogWarning("Попытка изменить несуществующий элемент заказа с ID: {OrderItemId}, в заказе с ID: {OrderId}", request.OrderItemId, request.OrderId);
                return MbResult<ShowOrderDto>.Failure(
                    [
                        new ErrorDetail("NOT_FOUND", $"Попытка изменить несуществующий элемент заказа с ID: {request.OrderItemId}, в заказе с ID: {request.OrderId}", "EditOrderItemCommand")
                    ]);
            }
                
            // Сохранение старых значений заказа
            Guid oldCarModelId = item.CarModelId;
            long oldQuantity = item.OrderQuantity;

            IExecutionStrategy strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.Execute(async () =>
            {
                // Открытие транзакции
                using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // если модель изменилась
                    if (request.CarModelId != null && request.CarModelId != oldCarModelId)
                    {
                        // возврат старой модель
                        await _carInventoryService.IncreaseInventoryAsync(oldCarModelId, (uint)oldQuantity);

                        // проверка доступности новой
                        CarInventoryServiceResult availability = await _carInventoryService.CheckAvailabilityAsync((Guid)request.CarModelId, (uint)request.OrderQuantity!);
                        if (!availability.IsAvailable || availability.AvailableQuantity < request.OrderQuantity)
                        {
                            _logger.LogWarning("Нет доступного количества машин с ID: {CarModelId}", request.CarModelId);
                            return MbResult<ShowOrderDto>.Failure(
                                [
                                    new ErrorDetail("NOT_FOUND", $"Нет доступного количества {request.OrderQuantity} машин с ID: {request.CarModelId}","EditOrderItemCommand")
                                ]);
                        }

                        // списывание новой
                        await _carInventoryService.DecreaseInventoryAsync((Guid)request.CarModelId, (uint)request.OrderQuantity!);

                        item.CarModelId = (Guid)request.CarModelId;
                        item.UnitPrice = await _orderRepository.GetCarPrice(item.CarModelId);
                    }

                    // если модель та же, но меняется количество
                    if (request.CarModelId == oldCarModelId)
                    {
                        long diff = (long)request.OrderQuantity! - oldQuantity;
                        if (diff > 0)
                        {
                            // проверка доступности на разницу
                            CarInventoryServiceResult availability = await _carInventoryService.CheckAvailabilityAsync(item.CarModelId, (uint)diff);
                            if (!availability.IsAvailable || availability.AvailableQuantity < diff)
                            {
                                _logger.LogWarning("Нет доступного количества {OrderQuantity} машин с ID: {CarModelId}", request.OrderQuantity, request.CarModelId);
                                return MbResult<ShowOrderDto>.Failure(
                                    [
                                        new ErrorDetail("NOT_FOUND",$"Нет доступного количества {request.OrderQuantity} машин с ID: {request.CarModelId}","EditOrderItemCommand")
                                    ]);
                            }
                            await _carInventoryService.DecreaseInventoryAsync(item.CarModelId, (uint)diff);
                        }
                        else if (diff < 0)
                        {
                            await _carInventoryService.IncreaseInventoryAsync(item.CarModelId, (uint)(-diff));
                        }
                    }

                    // обновление данных по заказу
                    item.OrderQuantity = (long)request.OrderQuantity!;
                    item.OrderItemComments = request.OrderItemComments;
                    order.OrderPrice = order.OrderItems?.Sum(item => item.UnitPrice * item.OrderQuantity) ?? 0;

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    ShowOrderDto result = _mapper.Map<ShowOrderDto>(order);

                    // Сохранение ключа идемпотентности
                    string json = JsonSerializer.Serialize(result);
                    await _idempotencyService.SaveAsync(request.IdempotencyKey, "OrderId", order.OrderId, json);

                    _logger.LogInformation("Элемент заказа с ID: {OrderItemId} в заказе с ID:{OrderId} успешно изменён", request.OrderItemId, request.OrderId);
                    return MbResult<ShowOrderDto>.Success(result);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Ошибка при попытке обновить данные элемента заказа с ID: {OrderItemId} заказа с ID {OrderId}", request.OrderItemId, request.OrderId);
                    return MbResult<ShowOrderDto>.Failure(
                        [
                            new ErrorDetail("Exception", $"{ex.Message}", "EditOrderItemCommand")
                        ]);
                }
            });

           
        }
    }
}
