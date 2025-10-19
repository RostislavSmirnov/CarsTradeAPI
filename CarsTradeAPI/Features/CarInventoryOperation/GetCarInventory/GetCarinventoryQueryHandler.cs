using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;
using MediatR;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryRepository;
using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.CarInventoryOperation.GetCarInventory
{
    public class GetCarinventoryQueryHandler : IRequestHandler<GetCarInventoryQuery, MbResult<List<ShowCarInventoryDto>>>
    {
        private readonly ICarInventoryRepository _carInventoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCarinventoryQueryHandler> _logger;
        private readonly ICacheService _cache;
        public GetCarinventoryQueryHandler(
            ICarInventoryRepository carInventoryRepository,
            IMapper mapper,
            ILogger<GetCarinventoryQueryHandler> logger,
            ICacheService cacheService)
        {
            _carInventoryRepository = carInventoryRepository;
            _mapper = mapper;
            _logger = logger;
            _cache = cacheService;
        }


        public async Task<MbResult<List<ShowCarInventoryDto>>> Handle(GetCarInventoryQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды GetCarInventoryQuery");

            // Проверка кэша
            string cacheKey = "car_Inventorys:all";
            List<ShowCarInventoryDto>? cached = await _cache.GetAsync<List<ShowCarInventoryDto>>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Получение {Count} CarInventory из кэша", cached.Count);
                return MbResult<List<ShowCarInventoryDto>>.Success(cached);
            }

            try
            {
                // Преобразование списка CarInventory в список ShowCarInventoryDto
                List<CarInventory> carInventories = await _carInventoryRepository.GetAllCarInventoriesAsync();
                List<ShowCarInventoryDto> result = _mapper.Map<List<ShowCarInventoryDto>>(carInventories);
                
                // Добавление информации о CarModel в каждый элемент результата
                foreach (var item in result)
                {
                    item.CarModel = _mapper.Map<ShowCarModelDto>(await _carInventoryRepository.GetCarModelByIdAsync(item.CarModelId));
                }

                // Сохранение в кэш
                await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Успешное получение списка CarInventory, количество элементов: {Count}", result.Count);
                return MbResult<List<ShowCarInventoryDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке получить список CarInventory");
                return MbResult<List<ShowCarInventoryDto>>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "GetCarInventoryQuery")
                });
            }
        }
    }
}
