using MediatR;
using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.CarModelOperation.CarModelRepository;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;
using CarsTradeAPI.Infrastructure.Services.CacheService;
using CarsTradeAPI.Infrastructure.Services.CarEngineFuelService;


namespace CarsTradeAPI.Features.CarModelOperation.CreateCarModel
{
    public class CreateCarModelCommandHandler : IRequestHandler<CreateCarModelCommand, MbResult<ShowCarModelDto>>
    {
        private readonly IMapper _mapper;
        private readonly ICarModelRepository _repository;
        private readonly ILogger<CreateCarModelCommandHandler> _logger;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ICacheService _cache;
        private readonly ICarEngineFuelService _carEngineFuelService;
        public CreateCarModelCommandHandler(
            IMapper mapper,
            ICarModelRepository repository,
            ILogger<CreateCarModelCommandHandler> logger,
            IIdempotencyService idempotencyService,
            ICacheService cacheService,
            ICarEngineFuelService fuelService)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
            _idempotencyService = idempotencyService;
            _cache = cacheService;
            _carEngineFuelService = fuelService;
        }

        
        public async Task<MbResult<ShowCarModelDto>> Handle(CreateCarModelCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды CreateCarModelCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                CarModel? existingCarModel = await _repository.GetCarModelByIdAsync(resourceId.Value);
                if (existingCarModel != null)
                {
                    ShowCarModelDto dto = _mapper.Map<ShowCarModelDto>(existingCarModel);
                    return MbResult<ShowCarModelDto>.Success(dto);
                }
                else
                {
                    _logger.LogWarning("Ключ идемпотенстности найден для ключа: {Key}, но сущность {CarModelId} не найдена в базе данных.", request.IdempotencyKey, resourceId.Value); 
                }
            }

            // Проверка типа топлива
            string? fuelType = _carEngineFuelService.CheckCarEngineFuelType(request.CarEngine!.FuelType);
            if (fuelType == null)
            {
                _logger.LogWarning("Недопустимый тип топлива: {FuelType}", request.CarEngine.FuelType);
                return MbResult<ShowCarModelDto>.Failure(new[]
                {
                    new ErrorDetail("VALIDATION_ERROR", $"Недопустимый тип топлива {request.CarEngine!.FuelType}", "CreateCarModelCommand")
                });
            }

            try
            {
                // Создание нового объекта CarModel
                CarModel modelToAddCarModel = _mapper.Map<CarModel>(request);
                CarModel newCarModel = await _repository.CreateCarModelAsync(modelToAddCarModel);
                _logger.LogInformation("Объект CarModel успешно создан с ID: {CarModelId}", newCarModel.CarModelId);

                // Возврат результата, c преобразованием в ShowCarModelDto
                ShowCarModelDto result = _mapper.Map<ShowCarModelDto>(newCarModel);

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "CarModel", newCarModel.CarModelId, json);

                // Удаление из кэша
                await _cache.RemoveAsync("Car_models:all");

                _logger.LogInformation("Объект CarModel успешно преобразован в ShowCarModelDto с ID: {CarModelId}", result.CarModelId);
                return MbResult<ShowCarModelDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании объекта CarModel");
                return MbResult<ShowCarModelDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "CreateCarModelCommand")
                });
            }
        }
    }
}
