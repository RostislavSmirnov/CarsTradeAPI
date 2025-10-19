using MediatR;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Entities;
using AutoMapper;
using CarsTradeAPI.Features.OrdersOperation.OrdersRepository;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.OrdersOperation.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, MbResult<ShowOrderDto>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<GetOrderByIdQueryHandler> _logger;
        private readonly ICacheService _cache;
        public GetOrderByIdQueryHandler(
            IMapper mapper,
            IOrderRepository orderRepository,
            ILogger<GetOrderByIdQueryHandler> logger,
            ICacheService cacheService)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _logger = logger;
            _cache = cacheService;
        }


        public async Task<MbResult<ShowOrderDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды GetOrderByIdQuery");

            // Проверка кэша
            string cacheKey = $"Order:{request.Id}";
            ShowOrderDto? cached = await _cache.GetAsync<ShowOrderDto>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Получение заказа с ID {Id} из кэша", request.Id);
                return MbResult<ShowOrderDto>.Success(cached);
            }

            try
            {
                // Проверка что такой заказ действительно сузествует
                Order? order = await _orderRepository.GetOrderByIdAsync(request.Id);
                if (order == null)
                {
                    _logger.LogWarning("Попытка получить несуществующий заказ с ID: {Id}", request.Id);
                    return MbResult<ShowOrderDto>.Failure(new[]
                    {
                        new ErrorDetail("NOT_FOUND", $"Заказ с ID: {request.Id} не найден", "GetOrderByIdQuery")
                    });
                }

                // Маппинг найденого заказа в Dto, и его возврат 
                ShowOrderDto result = _mapper.Map<ShowOrderDto>(order);

                // Сохранение в кэш
                await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Заказ с ID: {Id} успешно получен", request.Id);
                return MbResult<ShowOrderDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке получить заказ с ID: {Id}", request.Id);
                return MbResult<ShowOrderDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "GetOrderByIdQuery")
                });
            }
        }
    }
}
