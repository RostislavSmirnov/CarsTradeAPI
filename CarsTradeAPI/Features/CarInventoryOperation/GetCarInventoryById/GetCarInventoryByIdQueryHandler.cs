using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryRepository;
using MediatR;
using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.CarInventoryOperation.GetCarInventoryById
{
    public class GetCarInventoryByIdQueryHandler : IRequestHandler<GetCarInventoryByIdQuery, MbResult<ShowCarInventoryDto>>
    {
        private readonly ICarInventoryRepository _carInventoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCarInventoryByIdQueryHandler> _logger;
        private readonly ICacheService _cache;
        public GetCarInventoryByIdQueryHandler(
            ICarInventoryRepository carInventoryRepository,
            IMapper mapper,
            ILogger<GetCarInventoryByIdQueryHandler> logger,
            ICacheService cacheService)
        {
            _carInventoryRepository = carInventoryRepository;
            _mapper = mapper;
            _logger = logger;
            _cache = cacheService;
        }


        public async Task<MbResult<ShowCarInventoryDto>> Handle(GetCarInventoryByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки запроса GetCarInventoryByIdQuery");

            // Проверка кэша
            string cacheKey = $"car_Inventorys:{request.CarInventoryId}";
            ShowCarInventoryDto? cached = await _cache.GetAsync<ShowCarInventoryDto>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Получение CarInventory из кэша с ID {CarInventoryId}", request.CarInventoryId);
                return MbResult<ShowCarInventoryDto>.Success(cached);
            }

            // Получение объекта CarInventory по ID, и если объект не найден - возврат ошибки
            CarInventory? carInventory = await _carInventoryRepository.GetCarInventoryByIdAsync(request.CarInventoryId);
            if (carInventory == null) 
            {
                _logger.LogWarning("Объект CarInventory с ID: {CarInventoryId} не найден", request.CarInventoryId);
                return MbResult<ShowCarInventoryDto>.Failure(new[]
                {
                    new ErrorDetail("NOT_FOUND", $"Объект CarInventory с ID: {request.CarInventoryId} не найден", "GetCarInventoryByIdQuery")
                });
            }

            try
            {
                // Преобразование объекта в ShowCarInventoryDto для возврата результата
                ShowCarInventoryDto result = _mapper.Map<ShowCarInventoryDto>(carInventory);
                result.CarModel = _mapper.Map<ShowCarModelDto>(await _carInventoryRepository.GetCarModelByIdAsync(carInventory.CarModelId));

                // Сохранение в кэш
                await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Объект CarInventory с ID: {CarInventoryId} успешно получен", request.CarInventoryId);
                return MbResult<ShowCarInventoryDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке получить объект CarInventory с ID: {CarInventoryId}", request.CarInventoryId);
                return MbResult<ShowCarInventoryDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "GetCarInventoryByIdQuery")
                });
            }

        }
    }
}
