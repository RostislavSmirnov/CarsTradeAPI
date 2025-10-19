using MediatR;
using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using AutoMapper;
using CarsTradeAPI.Features.BuyerOperation.BuyerRepository;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.BuyerOperation.DeleteBuyer
{
    public class DeleteBuyerCommandHandler : IRequestHandler<DeleteBuyerCommand, MbResult<ShowBuyerDto>>
    {
        private readonly IBuyerRepository _buyerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DeleteBuyerCommandHandler> _logger;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ICacheService _cache;
        public DeleteBuyerCommandHandler(
            IBuyerRepository buyerRepository,
            IMapper mapper,
            ILogger<DeleteBuyerCommandHandler> logger,
            IIdempotencyService idempotencyService,
            ICacheService cacheService)
        {
            _buyerRepository = buyerRepository;
            _mapper = mapper;
            _logger = logger;
            _idempotencyService = idempotencyService;
            _cache = cacheService;
        }


        public async Task<MbResult<ShowBuyerDto>> Handle(DeleteBuyerCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды DeleteBuyerCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                Buyer? existingBuyer = await _buyerRepository.GetBuyerByIdAsync(resourceId.Value);
                ShowBuyerDto dto = _mapper.Map<ShowBuyerDto>(existingBuyer);
                return MbResult<ShowBuyerDto>.Success(dto);
            }
            
            try
            {
                // Запрос на удаление покупателя в репозитории
                Buyer? deletedBuyer = await _buyerRepository.DeleteBuyerAsync(request.BuyerId);
                if (deletedBuyer == null)
                {
                    _logger.LogWarning("Попытка удалить несуществующего покупателя с ID: {BuyerId}", request.BuyerId);
                    return MbResult<ShowBuyerDto>.Failure(new[]
                    {
                    new ErrorDetail("NOT_FOUND", $"Покупатель с ID: {request.BuyerId} не найден", "DeleteBuyerCommand")
                    });
                }

                _logger.LogInformation("Покупатель с ID: {BuyerId} успешно удален", request.BuyerId);

                // Маппинг удаленного покупателя в DTO для возврата результата
                ShowBuyerDto result = _mapper.Map<ShowBuyerDto>(deletedBuyer);

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "Buyer", deletedBuyer.BuyerId, json);

                // Удаление из кэша
                await _cache.RemoveAsync("Buyers:all");
                await _cache.RemoveAsync($"Buyers:{request.BuyerId}");

                _logger.LogInformation("Возврат результата удаления покупателя с ID: {BuyerId}", request.BuyerId);
                return MbResult<ShowBuyerDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении покупателя с ID: {BuyerId}", request.BuyerId);
                return MbResult<ShowBuyerDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "DeleteBuyerCommand")
                });
            }
        }
    }
}
