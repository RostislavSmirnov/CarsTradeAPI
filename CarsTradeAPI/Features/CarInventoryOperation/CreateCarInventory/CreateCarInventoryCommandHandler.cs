using MediatR;
using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryRepository;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.CarInventoryOperation.CreateCarInventory
{
    public class CreateCarInventoryCommandHandler : IRequestHandler<CreateCarInventoryCommand, MbResult<ShowCarInventoryDto>>
    {
        private readonly ICarInventoryRepository _carInventoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateCarInventoryCommandHandler> _logger;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ICacheService _cache;
        public CreateCarInventoryCommandHandler(
            ICarInventoryRepository carInventoryRepository,
            IMapper mapper,
            ILogger<CreateCarInventoryCommandHandler> logger,
            IIdempotencyService idempotencyService,
            ICacheService cacheService)
        {
            _carInventoryRepository = carInventoryRepository;
            _mapper = mapper;
            _logger = logger;
            _idempotencyService = idempotencyService;
            _cache = cacheService;
        }


        public async Task<MbResult<ShowCarInventoryDto>> Handle(CreateCarInventoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды CreateCarInventoryCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                CarInventory? existingCarInventory = await _carInventoryRepository.GetCarInventoryByIdAsync(resourceId.Value);
                ShowCarInventoryDto dto = _mapper.Map<ShowCarInventoryDto>(existingCarInventory);
                return MbResult<ShowCarInventoryDto>.Success(dto);
            }

            // Проверка существования CarModel по CarModelId
            CarModel? carModel = await _carInventoryRepository.GetCarModelByIdAsync(request.CarModelId);
            if (carModel == null) 
            {
                _logger.LogWarning("Попытка создать объект CarInventory с несуществующим CarModelId: {CarModelId}", request.CarModelId);
                return MbResult<ShowCarInventoryDto>.Failure(new[]
                {
                    new ErrorDetail("NOT_FOUND", $"Автомобильная модель с ID: {request.CarModelId} не найдена", "CreateCarInventoryCommand")
                });
            }

            try
            {
                // Создание нового объекта CarInventory
                CarInventory carInventory = new CarInventory
                {
                    InventoryId = Guid.NewGuid(),
                    LastUpdated = DateTime.UtcNow,
                };
                 _mapper.Map(request, carInventory);

                // Сохранение нового объекта CarInventory в репозитории 
                CarInventory createdCarInventory = await _carInventoryRepository.CreateCarInventoryAsync(carInventory);
                _logger.LogInformation("Объект CarInventory успешно создан с ID: {InventoryId}", createdCarInventory.InventoryId);

                // Возврат результата, c преобразованием в ShowCarInventoryDto
                ShowCarInventoryDto result = _mapper.Map<ShowCarInventoryDto>(createdCarInventory);
                result.CarModel = _mapper.Map<ShowCarModelDto>(carModel);

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "CarInventory", createdCarInventory.InventoryId, json);

                // Удаление из кэша
                await _cache.RemoveAsync("car_Inventorys:all");

                _logger.LogInformation("Объект CarInventory с ID: {InventoryId} успешно создан", result.InventoryId);
                return MbResult<ShowCarInventoryDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании объекта CarInventory");
                return MbResult<ShowCarInventoryDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "CreateCarInventoryCommand")
                });
            }
        }
    }
}
