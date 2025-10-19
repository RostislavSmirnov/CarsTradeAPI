using MediatR;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Entities;
using AutoMapper;
using CarsTradeAPI.Features.OrdersOperation.OrdersRepository;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.OrdersOperation.EditOrder
{
    public class EditOrderCommandHandler : IRequestHandler<EditOrderCommand, MbResult<ShowOrderDto>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<EditOrderCommandHandler> _logger;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ICacheService _cache;
        public EditOrderCommandHandler(
            IMapper mapper,
            IOrderRepository orderRepository,
            ILogger<EditOrderCommandHandler> logger,
            IIdempotencyService idempotencyService,
            ICacheService cacheService)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _logger = logger;
            _idempotencyService = idempotencyService;
            _cache = cacheService;
        }


        public async Task<MbResult<ShowOrderDto>> Handle(EditOrderCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Нало обработки комманды EditOrderCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                Order? existingOrder = await _orderRepository.GetOrderByIdAsync(resourceId.Value);
                ShowOrderDto dto = _mapper.Map<ShowOrderDto>(existingOrder);
                return MbResult<ShowOrderDto>.Success(dto);
            }

            // Проверка что такой заказ действительно существует
            Order? order = await _orderRepository.GetOrderByIdAsync((Guid)request.OrderId!);
            if (order == null)
            {
                _logger.LogWarning("Попытка изменить несуществующий заказ с ID: {OrderId}", request.OrderId);
                return MbResult<ShowOrderDto>.Failure(new[] 
                {
                    new ErrorDetail("NOT_FOUND", $"Заказ с ID: {request.OrderId} не найден", "EditOrderCommand")
                });
            }

            // Если в команде запрос на изменение ID заказчика, то проверяем его наличие
            if (request.BuyerId != null) 
            {
                // Проверка что такой покупатель действительно существует
                bool result = await _orderRepository.CheckOrderBuyerAsync((Guid)request.BuyerId);
                if (result == false)
                {
                    _logger.LogWarning("Попытка изменить на несуществующего заказчика  ID: {BuyerId}", request.BuyerId);
                    return MbResult<ShowOrderDto>.Failure(new[]
                    {
                        new ErrorDetail("NOT_FOUND", $"Покупатель с ID: {request.BuyerId} не найден","EditOrderCommand")
                    });    
                }             
            }

            // Если в команде запрос на изменение ID работника, то проверяем его наличие
            if (request.EmployeeId != null)
            {
                // Проверка что такой работник действительно существует
                bool result = await _orderRepository.CheckOrderEmployeeAsync((Guid)request.EmployeeId);
                if (result == false)
                {
                    _logger.LogWarning("Попытка изменить на несуществующего работника  ID: {Employee}", request.EmployeeId);
                    return MbResult<ShowOrderDto>.Failure(new[]
                    {
                        new ErrorDetail("NOT_FOUND", $"работник с ID: {request.EmployeeId} не найден","EditOrderCommand")
                    });
                }
            }

            try
            {   
                // Маппинг полей, и сохранение в бд, с преобразованием в Dto для возврата 
                Order updatedOrder = _mapper.Map(request, order);
                Order editedOrder = await _orderRepository.EditOrderAsync(updatedOrder);
                ShowOrderDto result = _mapper.Map<ShowOrderDto>(editedOrder);

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "Order", editedOrder.OrderId, json);

                // Удаление из кэша
                await _cache.RemoveAsync($"Order:{request.OrderId}");
                await _cache.RemoveAsync("Order:all");

                _logger.LogInformation("Заказ успешно создан с ID: {OrderId}", editedOrder.OrderId);
                return MbResult<ShowOrderDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке обновить данные заказа с ID {OrderId}", request.OrderId);
                return MbResult<ShowOrderDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "EditOrderCommand")
                });
            }
        }
    }
}
