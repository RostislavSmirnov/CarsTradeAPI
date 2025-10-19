using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Features.CarModelOperation.CarModelRepository;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;
using CarsTradeAPI.Infrastructure.Services.CacheService;
using CarsTradeAPI.Infrastructure.Services.CarEngineFuelService;


namespace CarsTradeAPI.Features.CarModelOperation.EditCarModel
{
    public class EditCarModelCommandHandler : IRequestHandler<EditCarModelCommand, MbResult<ShowCarModelDto>>
    {
        private readonly ICarModelRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<EditCarModelCommandHandler> _logger;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ICacheService _cache;
        private readonly ICarEngineFuelService _fuelService;
        public EditCarModelCommandHandler(
            ICarModelRepository repository,
            IMapper mapper,
            ILogger<EditCarModelCommandHandler> logger,
            IIdempotencyService idempotencyService,
            ICacheService cacheService,
            ICarEngineFuelService fuelService)

        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _idempotencyService = idempotencyService;
            _cache = cacheService;
            _fuelService = fuelService;
        }


        public async Task<MbResult<ShowCarModelDto>> Handle(EditCarModelCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды EditCarModelCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                CarModel? existingCarModel = await _repository.GetCarModelByIdAsync(resourceId.Value);
                ShowCarModelDto dto = _mapper.Map<ShowCarModelDto>(existingCarModel);
                return MbResult<ShowCarModelDto>.Success(dto);
            }

            // Проверка существования CarModel по CarModelId
            CarModel? carModel = await _repository.GetCarModelByIdAsync((Guid)request.CarModelId!);
            if (carModel == null) 
            {
                _logger.LogWarning("Попытка изменить несуществующую модель автомобиля с ID: {CarModelId}", request.CarModelId);
                return MbResult<ShowCarModelDto>.Failure(new[]
                {
                    new ErrorDetail("NOT_FOUND", $"Модель автомобиля с ID: {request.CarModelId} не найдена", "EditCarModelCommand")
                });
            }

            // Валидация типа топлива, если оно было передано в запросе
            if (request.CarEngine!.FuelType != null) 
            {
                _logger.LogInformation("Проверка типа топлива: {FuelType}", request.CarEngine.FuelType);
                string? fuelType = _fuelService.CheckCarEngineFuelType(request.CarEngine.FuelType);
                if (fuelType == null)
                {
                    _logger.LogWarning("Недопустимый тип топлива: {FuelType}", request.CarEngine.FuelType);
                    return MbResult<ShowCarModelDto>.Failure(new[]
                    {
                        new ErrorDetail("VALIDATION_ERROR", $"Недопустимый тип топлива {request.CarEngine.FuelType}", "EditCarModelCommand")
                    });
                }
            }

            try
            {
                // Маппинг полей из команды в сущность CarModel, после чего обновление объекта в репозитории.
                CarModel updatedModel = _mapper.Map(request, carModel);
                CarModel updatedCarModel = await _repository.EditCarModelAsync(updatedModel);
                _logger.LogInformation("Модель автомобиля с ID: {CarModelId} успешно обновлена", request.CarModelId);
                
                // Маппинг обновленного объекта в ShowCarModelDto для возврата результата
                ShowCarModelDto result = _mapper.Map<ShowCarModelDto>(updatedCarModel);

                 // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "CarModel", updatedCarModel.CarModelId, json);

                // Удаление из кэша
                await _cache.RemoveAsync("Car_models:all");
                await _cache.RemoveAsync($"Car_models:{request.CarModelId}");

                _logger.LogInformation("Модель автомобиля с ID: {CarModelId} успешно преобразована в ShowCarModelDto", request.CarModelId);
                return MbResult<ShowCarModelDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке обновить модель автомобиля с ID: {CarModelId}", request.CarModelId);
                return MbResult<ShowCarModelDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "EditCarModelCommand")
                });
            }
        }
    }
}
