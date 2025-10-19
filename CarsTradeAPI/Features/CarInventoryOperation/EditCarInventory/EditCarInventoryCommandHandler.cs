using MediatR;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryRepository;
using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using System.Text.Json;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.CarInventoryOperation.EditCarInventory
{
    public class EditCarInventoryCommandHandler : IRequestHandler<EditCarInventoryCommand, MbResult<ShowCarInventoryDto>>
    {
        private readonly ICarInventoryRepository _carInventoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EditCarInventoryCommandHandler> _logger;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ICacheService _cache;
        public EditCarInventoryCommandHandler(
            ICarInventoryRepository carInventoryRepository,
            IMapper mapper,
            ILogger<EditCarInventoryCommandHandler> logger,
            IIdempotencyService idempotencyService,
            ICacheService cacheService)
        {
            _carInventoryRepository = carInventoryRepository;
            _mapper = mapper;
            _logger = logger;
            _idempotencyService = idempotencyService;
            _cache = cacheService;
        }


        public async Task<MbResult<ShowCarInventoryDto>> Handle(EditCarInventoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды EditCarInventoryCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                CarInventory? existingCarInventory = await _carInventoryRepository.GetCarInventoryByIdAsync(resourceId.Value);
                ShowCarInventoryDto dto = _mapper.Map<ShowCarInventoryDto>(existingCarInventory);
                return MbResult<ShowCarInventoryDto>.Success(dto);
            }

            // Проверка существования объекта CarInventory
            CarInventory? carModel = await _carInventoryRepository.GetCarInventoryByIdAsync(request.InventoryId);
            if (carModel == null)
            {
                _logger.LogWarning("Попытка изменить несуществующий объект с ID: {InventoryId}", request.InventoryId);
                return MbResult<ShowCarInventoryDto>.Failure(new[]
                {
                    new ErrorDetail("NOT_FOUND", $"Объект с ID: {request.InventoryId} не найден", "EditCarInventoryCommand")
                });
            }

            try
            {
                // Маппинг полей из команды в сущность CarInventory, после чего обновление объекта в репозитории, и возврат результата в DTO
                CarInventory editedCarInventory = _mapper.Map(request, carModel);
                ShowCarInventoryDto result = _mapper.Map<ShowCarInventoryDto>(await _carInventoryRepository.EditCarInventoryAsync(editedCarInventory));
                result.CarModel = _mapper.Map<ShowCarModelDto>(await _carInventoryRepository.GetCarModelByIdAsync(editedCarInventory.CarModelId));

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "CarInventory", editedCarInventory.InventoryId, json);

                // Удаление из Кэша
                await _cache.RemoveAsync("car_Inventorys:all");
                await _cache.RemoveAsync($"car_Inventorys:{request.InventoryId}");

                _logger.LogInformation("Объект CarInventory с ID: {InventoryId} успешно обновлен", request.InventoryId);
                return MbResult<ShowCarInventoryDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке обновить объект CarInventory с ID: {InventoryId}", request.InventoryId);
                return MbResult<ShowCarInventoryDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "EditCarInventoryCommand")
                });
            }
        }
    }
}
