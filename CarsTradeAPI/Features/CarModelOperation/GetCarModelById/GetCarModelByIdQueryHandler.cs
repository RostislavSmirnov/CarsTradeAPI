using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Features.CarModelOperation.CarModelRepository;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.CarModelOperation.GetCarModelById
{
    public class GetCarModelByIdQueryHandler : IRequestHandler<GetCarModelByIdQuery, MbResult<ShowCarModelDto>>
    {
        private readonly ICarModelRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCarModelByIdQueryHandler> _logger;
        private readonly ICacheService _cache;
        public GetCarModelByIdQueryHandler(ICarModelRepository repository, IMapper mapper, ILogger<GetCarModelByIdQueryHandler> logger, ICacheService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }


        public async Task<MbResult<ShowCarModelDto>> Handle(GetCarModelByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки запроса GetCarModelByIdQuery");

            // Проверка кэша
            string cacheKey = $"Car_models:{request.CarModelId}";
            ShowCarModelDto? cached = await _cache.GetAsync<ShowCarModelDto>(cacheKey);
            if (cached != null) 
            {
                _logger.LogInformation("Получение модели автомобиля с ID {CarMopdelId} из кэша", request.CarModelId);
                return MbResult<ShowCarModelDto>.Success(cached);
            }

            try
            {
                // Проверка существования модели автомобиля по ID
                CarModel? carModel = await _repository.GetCarModelByIdAsync(request.CarModelId);
                if (carModel == null)
                {
                    _logger.LogWarning("Попытка получить несуществующую модель автомобиля с ID: {CarModelId}", request.CarModelId);
                    return MbResult<ShowCarModelDto>.Failure(new[]
                    {
                    new ErrorDetail("NOT_FOUND", $"Модель автомобиля с ID: {request.CarModelId} не найдена", "GetCarModelByIdQuery")
                    });
                }

                // Преобразование сущности CarModel в ShowCarModelDto для возврата результата
                ShowCarModelDto result = _mapper.Map<ShowCarModelDto>(carModel);

                // Сохранение в кэш
                await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Модель автомобиля с ID: {CarModelId} успешно получена", request.CarModelId);
                return MbResult<ShowCarModelDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке получить модель автомобиля с ID: {CarModelId}", request.CarModelId);
                return MbResult<ShowCarModelDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "GetCarModelByIdQuery")
                });
            }
        }
    }
}
