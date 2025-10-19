using MediatR;
using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using AutoMapper;
using CarsTradeAPI.Features.BuyerOperation.BuyerRepository;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.BuyerOperation.GetBuyerById
{
    public class GetBuyerByIdQueryHandler : IRequestHandler<GetBuyerByIdQuery, MbResult<ShowBuyerDto>>
    {
        private readonly IMapper _mapper;
        private readonly IBuyerRepository _buyerRepository;
        private readonly ILogger<GetBuyerByIdQueryHandler> _logger;
        private readonly ICacheService _cache;
        public GetBuyerByIdQueryHandler(
            IBuyerRepository buyerRepository,
            IMapper mapper,
            ILogger<GetBuyerByIdQueryHandler> logger,
            ICacheService cacheService)
        {
            _buyerRepository = buyerRepository;
            _mapper = mapper;
            _logger = logger;
            _cache = cacheService;
        }


        public async Task<MbResult<ShowBuyerDto>> Handle(GetBuyerByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки запроса GetBuyerByIdQuery");

            // Проверка кэша
            string cacheKey = $"Buyers:{request.BuyerId}";
            ShowBuyerDto? cached = await _cache.GetAsync<ShowBuyerDto>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Получение покупателz c ID: {BuyerId} из кэша", request.BuyerId);
                return MbResult<ShowBuyerDto>.Success(cached);
            }

            // Проверка существования покупателя
            _logger.LogInformation("Проверка существования покупателя с ID: {BuyerId}", request.BuyerId);
            Buyer? buyer = await _buyerRepository.GetBuyerByIdAsync(request.BuyerId);
            if (buyer == null)
            {
                _logger.LogWarning("Попытка получить несуществующего покупателя с ID: {BuyerId}", request.BuyerId);
                return MbResult<ShowBuyerDto>.Failure(new[]
                {
                    new ErrorDetail("NOT_FOUND", $"Покупатель с ID: {request.BuyerId} не найден", "GetBuyerByIdQuery")
                });
            }

            try
            {
                // Маппинг сущности покупателя в DTO для возврата результата
                ShowBuyerDto result = _mapper.Map<ShowBuyerDto>(buyer);
                
                // Сохранение в кэш
                await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                return MbResult<ShowBuyerDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении покупателя с ID: {BuyerId}", request.BuyerId);
                return MbResult<ShowBuyerDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "GetBuyerByIdQuery")
                });
            }
        }

    }
}
