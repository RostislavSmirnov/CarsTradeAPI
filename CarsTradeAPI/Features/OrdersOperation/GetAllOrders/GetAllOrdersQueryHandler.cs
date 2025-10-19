using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Features.OrdersOperation.OrdersRepository;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.CacheService;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;


namespace CarsTradeAPI.Features.OrdersOperation.GetAllOrders
{
    public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, MbResult<List<ShowOrderDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<GetAllOrdersQueryHandler> _logger;
        private readonly ICacheService _cache;
        public GetAllOrdersQueryHandler(IMapper mapper, IOrderRepository orderRepository, ILogger<GetAllOrdersQueryHandler> logger, ICacheService cacheService)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _logger = logger;
            _cache = cacheService;
        }


        public async Task<MbResult<List<ShowOrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды GetAllOrdersQuery");

            // Проверка кэша
            string cacheKey = "Order:all";
            List<ShowOrderDto>? cached = await _cache.GetAsync<List<ShowOrderDto>>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Получение {Count} заказов из кэша", cached.Count);
                return MbResult<List<ShowOrderDto>>.Success(cached);
            }

            try
            {
                // получение всех заказов, и маппинг их в Dto
                List<Order> orders = await _orderRepository.GetAllOrdersAsync();
                List<ShowOrderDto> result = _mapper.Map<List<ShowOrderDto>>(orders);

                // Сохранение в кэш
                await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Успешное получение {Count} заказов", result.Count());
                return MbResult<List<ShowOrderDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех заказов");
                return MbResult<List<ShowOrderDto>>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "GetAllOrdersQuery")
                });
            }
        }
    }
}
