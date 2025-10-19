using MediatR;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Entities;
using AutoMapper;
using CarsTradeAPI.Features.OrdersOperation.OrdersRepository;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.OrdersOperation.DeleteOrder
{
    public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, MbResult<ShowOrderDto>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<DeleteOrderCommandHandler> _logger;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ICacheService _cache;
        public DeleteOrderCommandHandler(
            IMapper mapper,
            IOrderRepository orderRepository,
            ILogger<DeleteOrderCommandHandler> logger,
            IIdempotencyService idempotencyService,
            ICacheService cache)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _logger = logger;
            _idempotencyService = idempotencyService;
            _cache = cache;
        }
        

        public async Task<MbResult<ShowOrderDto>> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды DeleteOrderCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                Order? existingOrder = await _orderRepository.GetOrderByIdAsync(resourceId.Value);
                ShowOrderDto dto = _mapper.Map<ShowOrderDto>(existingOrder);
                return MbResult<ShowOrderDto>.Success(dto);
            }

            try
            {
                // Удаление заказа из репозитория, и возврат результата, c преобразованием в ShowOrderDto
                Order? deletedOrder = await _orderRepository.DeleteOrderAsync(request.OrderId);
                if (deletedOrder == null)
                {
                    _logger.LogWarning("Попытка удалить несуществующий заказ с ID: {OrderId}", request.OrderId);
                    return MbResult<ShowOrderDto>.Failure(new[]
                    {
                        new ErrorDetail("NOT_FOUND", $"Заказ с ID: {request.OrderId} не найден", "DeleteOrderCommand")
                    });
                }

                ShowOrderDto result = _mapper.Map<ShowOrderDto>(deletedOrder);

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "Order", deletedOrder.OrderId, json);

                // Удаление из кэша
                await _cache.RemoveAsync($"Order:{request.OrderId}");
                await _cache.RemoveAsync("Order:all");

                _logger.LogInformation("Заказ с ID: {OrderId} успешно удален", request.OrderId);
                return MbResult<ShowOrderDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке удалить заказ с ID: {OrderId}", request.OrderId);
                return MbResult<ShowOrderDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "DeleteOrderCommand")
                });
            }
        }
    }
}
