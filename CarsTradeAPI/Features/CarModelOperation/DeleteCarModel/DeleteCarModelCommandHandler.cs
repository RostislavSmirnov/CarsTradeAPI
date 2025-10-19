using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.CarModelOperation.CarModelRepository;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using AutoMapper;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.CarModelOperation.DeleteCarModel
{
    public class DeleteCarModelCommandHandler : IRequestHandler<DeleteCarModelCommand, MbResult<ShowCarModelDto>>
    {
        private readonly ICarModelRepository _repository;
        private readonly ILogger<DeleteCarModelCommandHandler> _logger;
        private readonly IMapper _mapper;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ICacheService _cache;
        public DeleteCarModelCommandHandler(
            ICarModelRepository repository,
            ILogger<DeleteCarModelCommandHandler> logger,
            IMapper mapper,
            IIdempotencyService idempotencyService,
            ICacheService cacheService)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _idempotencyService = idempotencyService;
            _cache = cacheService;
        }


        public async Task<MbResult<ShowCarModelDto>> Handle(DeleteCarModelCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды DeleteCarModelCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                CarModel? existingCarModel = await _repository.GetCarModelByIdAsync(resourceId.Value);
                ShowCarModelDto dto = _mapper.Map<ShowCarModelDto>(existingCarModel);
                return MbResult<ShowCarModelDto>.Success(dto);
            }

            try
            {
                // Проверка существования модели по ID, и удаление модели по ID, если она существует
                CarModel? deletedCarModel = await _repository.DeleteCarModelAsync(request.CarModelId);
                if (deletedCarModel == null)
                {
                    _logger.LogWarning("Попытка удалить несуществующую модель с ID: {CarModelId}", request.CarModelId);
                    return MbResult<ShowCarModelDto>.Failure(new[]
                    {
                        new ErrorDetail("NOT_FOUND", $"Модель с ID: {request.CarModelId} не найдена", "DeleteCarModelCommand")
                    });
                }

                _logger.LogInformation("Модель с ID: {CarModelId} успешно удалена", request.CarModelId);

                // Преобразование удаленного объекта в ShowCarModelDto для возврата результата
                ShowCarModelDto result = _mapper.Map<ShowCarModelDto>(deletedCarModel);

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "CarModel", deletedCarModel.CarModelId, json);

                // Удаление из кэша
                await _cache.RemoveAsync("Car_models:all");
                await _cache.RemoveAsync($"Car_models:{request.CarModelId}");

                _logger.LogInformation("Возврат результата удаления модели с ID: {CarModelId}", request.CarModelId);
                return MbResult<ShowCarModelDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении модели с ID: {CarModelId}", request.CarModelId);
                return MbResult<ShowCarModelDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "DeleteCarModelCommand")
                });
            }
        }
    }
}
