using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using MediatR;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryRepository;
using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.CarInventoryOperation.DeleteCarInventory
{
    public class DeleteCarInventoryCommandHandler : IRequestHandler<DeleteCarInventoryCommand, MbResult<ShowCarInventoryDto>>
    {
        private readonly ICarInventoryRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<DeleteCarInventoryCommandHandler> _logger;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ICacheService _cache;
        public DeleteCarInventoryCommandHandler(
            ICarInventoryRepository repository,
            IMapper mapper,
            ILogger<DeleteCarInventoryCommandHandler> logger,
            IIdempotencyService idempotencyService,
            ICacheService cacheService)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _idempotencyService = idempotencyService;
            _cache = cacheService;
        }


        public async Task<MbResult<ShowCarInventoryDto>> Handle(DeleteCarInventoryCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды DeleteCarInventoryCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                CarInventory? existingCarInventory = await _repository.GetCarInventoryByIdAsync(resourceId.Value);
                ShowCarInventoryDto dto = _mapper.Map<ShowCarInventoryDto>(existingCarInventory);
                return MbResult<ShowCarInventoryDto>.Success(dto);
            }

            try
            {
                // Удаление объекта CarInventory по ID, и если объект не найден - выброс исключения
                CarInventory? deletedCarInventory = await _repository.DeleteCarInventoryAsync(request.CarInventoryId);
                if (deletedCarInventory == null) 
                {
                    return MbResult<ShowCarInventoryDto>.Failure(new[]
                    {
                        new ErrorDetail("NOT_FOUND", $"Объект CarInventory с ID: {request.CarInventoryId} не найден", "DeleteCarInventoryCommand")
                    });
                }
                _logger.LogInformation("Объект CarInventory с ID: {InventoryId} успешно удален", request.CarInventoryId);
                
                // Преобразование удаленного объекта в ShowCarInventoryDto для возврата результата
                ShowCarInventoryDto result = _mapper.Map<ShowCarInventoryDto>(deletedCarInventory);
                result.CarModel = _mapper.Map<ShowCarModelDto>(await _repository.GetCarModelByIdAsync(deletedCarInventory.CarModelId));

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "CarInventory", deletedCarInventory.InventoryId, json);

                // Удаление из кэша
                await _cache.RemoveAsync("car_Inventorys:all");
                await _cache.RemoveAsync($"car_Inventorys:{request.CarInventoryId}");

                _logger.LogInformation("Объект CarInventory с ID: {InventoryId} успешно удалён", request.CarInventoryId);
                return MbResult<ShowCarInventoryDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении объекта CarInventory с ID: {InventoryId}", request.CarInventoryId);
                return MbResult<ShowCarInventoryDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "DeleteCarInventoryCommand")
                });
            }
        }
    }
}
