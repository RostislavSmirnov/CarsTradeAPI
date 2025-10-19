using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Features.CarModelOperation.CarModelRepository;
using AutoMapper;


namespace CarsTradeAPI.Features.CarModelOperation.SearchCarModel
{
    public class SearchCarModelQueryHandler : IRequestHandler<SearchCarModelQuery, MbResult<List<ShowCarModelDto>>>
    {
        private readonly ICarModelRepository _carModelRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SearchCarModelQueryHandler> _logger;
        public SearchCarModelQueryHandler(ICarModelRepository carModelRepository, IMapper mapper, ILogger<SearchCarModelQueryHandler> logger)
        {
            _carModelRepository = carModelRepository;
            _mapper = mapper;
            _logger = logger;
        }


        public async Task<MbResult<List<ShowCarModelDto>>> Handle(SearchCarModelQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки запроса SearchCarModelQuery");
            // Ответ, если был указан точный ID модели автомобиля.
            if (request.CarModelId != null)
            {
                CarModel? idResult = await _carModelRepository.GetCarModelByIdAsync((Guid)request.CarModelId);
                if (idResult == null)
                {
                    _logger.LogWarning("Попытка получить несуществующую модель автомобиля с ID: {CarModelId}", request.CarModelId);
                    return MbResult<List<ShowCarModelDto>>.Failure(
                        [
                            new ErrorDetail("NOT_FOUND", $"Попытка получить несуществующую модель автомобиля с ID: {request.CarModelId}", "SearchCarModelQuery")
                        ]);
                }
            }
            try
            {
                List<CarModel> carModels = await _carModelRepository.SearchCarModels(request);
                List<ShowCarModelDto> result = _mapper.Map<List<ShowCarModelDto>>(carModels);
                int count = result.Count;
                _logger.LogInformation("Запрос на поиск автомобилей был успешно выполнен, количество найдены автомобилей {count}", count);
                return MbResult<List<ShowCarModelDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Ошибка при попытке найти автомобили по указаным параметрам");
                return MbResult<List<ShowCarModelDto>>.Failure(
                    [
                        new ErrorDetail("Exception", $"Ошибка при попытке найти автомобили по указаным параметрам {ex.Message}", "SearchCarModelQuery")
                    ]);
            }
            
        }
    }
}
