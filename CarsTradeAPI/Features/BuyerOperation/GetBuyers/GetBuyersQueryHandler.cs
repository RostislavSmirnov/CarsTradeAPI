using MediatR;
using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using CarsTradeAPI.Features.BuyerOperation.BuyerRepository;
using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.CacheService;

namespace CarsTradeAPI.Features.BuyerOperation.GetBuyers
{
    public class GetBuyersQueryHandler : IRequestHandler<GetBuyersQuery, MbResult<List<ShowBuyerDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IBuyerRepository _buyerRepository;
        private readonly ILogger<GetBuyersQueryHandler> _logger;
        private readonly ICacheService _cache;
        public GetBuyersQueryHandler(IBuyerRepository buyerRepository, IMapper mapper, ILogger<GetBuyersQueryHandler> logger, ICacheService cacheService)
        {
            _buyerRepository = buyerRepository;
            _mapper = mapper;
            _logger = logger;
            _cache = cacheService;
        }


        public async Task<MbResult<List<ShowBuyerDto>>> Handle(GetBuyersQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки запроса GetBuyersQuery");

            // Проверка кэша
            string cacheKey = "Buyers:all";
            List<ShowBuyerDto>? cached = await _cache.GetAsync<List<ShowBuyerDto>>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Получение всех покупателей в количестве {Count}", cached.Count);
                return MbResult<List<ShowBuyerDto>>.Success(cached);
            }

            try
            {
                // Получение всех покупателей из репозитория
                List<Buyer> buyers = await _buyerRepository.GetAllBuyersAsync();
                _logger.LogInformation("Успешное получение {Count} покупателей", buyers.Count);

                // Маппинг сущностей покупателей в DTO для возврата результата
                List<ShowBuyerDto> result = _mapper.Map<List<ShowBuyerDto>>(buyers);

                // Сохранение в кэш
                await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                return MbResult<List<ShowBuyerDto>>.Success(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка покупателей");
                return MbResult<List<ShowBuyerDto>>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "GetBuyersQuery")
                });
            }
        }
    }
}
