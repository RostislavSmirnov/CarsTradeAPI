using AutoMapper;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Features.CarModelOperation.CarModelRepository;
using MediatR;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.CarModelOperation.GetCarModel
{
    public class GetCarModelQueryHandler : IRequestHandler<GetCarModelQuery, MbResult<List<ShowCarModelDto>>>
    {
        private readonly ICarModelRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCarModelQueryHandler> _logger;
        private readonly ICacheService _cache;
        public GetCarModelQueryHandler(
            ICarModelRepository repository,
            IMapper mapper,
            ILogger<GetCarModelQueryHandler> logger,
            ICacheService cache)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
            _cache = cache;
        }


        public async Task<MbResult<List<ShowCarModelDto>>> Handle(GetCarModelQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды GetCarModelQuery");
            try
            {
                // Проверка кэша
                string cacheKey = "Car_models:all";
                List<ShowCarModelDto>? cached = await _cache.GetAsync<List<ShowCarModelDto>>(cacheKey);
                if (cached != null) 
                {
                    _logger.LogInformation("Получение {Count} моделей автомобилей из кэша", cached.Count);
                    return MbResult<List<ShowCarModelDto>>.Success(cached);
                }

                // Получение всех моделей автомобилей из репозитория и преобразование их в ShowCarModelDto для возврата результата
                List<CarModel> carModels = await _repository.GetCarModelsAsync();
                List<ShowCarModelDto> result = _mapper.Map<List<ShowCarModelDto>>(carModels);

                // Сохранение в кэш
                await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Успешное получение {Count} моделей автомобилей", result.Count());
                return MbResult<List<ShowCarModelDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении моделей автомобилей");
                return MbResult<List<ShowCarModelDto>>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "GetCarModelQuery")
                });
            }
        }
    }
}
