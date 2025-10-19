using MediatR;
using CarsTradeAPI.Features.BuyerOperation.BuyerRepository;
using AutoMapper;
using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.BuyerOperation.EditBuyer
{
    public class EditBuyerCommandHandler : IRequestHandler<EditBuyerCommand, MbResult<ShowBuyerDto>>
    {
        private readonly IBuyerRepository _buyerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EditBuyerCommandHandler> _logger;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ICacheService _cache;
        public EditBuyerCommandHandler(
            IBuyerRepository buyerRepository,
            IMapper mapper,
            ILogger<EditBuyerCommandHandler> logger,
            IIdempotencyService idempotencyService,
            ICacheService cacheService)
        {
            _buyerRepository = buyerRepository;
            _mapper = mapper;
            _logger = logger;
            _idempotencyService = idempotencyService;
            _cache = cacheService;
        }


        public async Task<MbResult<ShowBuyerDto>> Handle(EditBuyerCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды EditBuyerCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                Buyer? existingBuyer = await _buyerRepository.GetBuyerByIdAsync(resourceId.Value);
                ShowBuyerDto dto = _mapper.Map<ShowBuyerDto>(existingBuyer);
                return MbResult<ShowBuyerDto>.Success(dto);
            }

            // Проверка существования покупателя
            _logger.LogInformation("Проверка существования покупателя с ID: {BuyerId}", request.BuyerId);
            Buyer? buyer = await _buyerRepository.GetBuyerByIdAsync(request.BuyerId);
            if (buyer == null)
            {
                _logger.LogWarning("Попытка изменить несуществующего покупателя с ID: {BuyerId}", request.BuyerId);
                return MbResult<ShowBuyerDto>.Failure(new[]
                {
                    new ErrorDetail("NOT_FOUND", $"Покупатель с ID: {request.BuyerId} не найден", "EditBuyerCommand")
                });
            }

            // Проверка на уникальность номера телефона
            _logger.LogInformation("Проверка уникальности номера телефона: {PhoneNumber}", request.PhoneNumber);
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                bool checkPhoneNumber = await _buyerRepository.CheckBuyerPhoneNumberAsync(request.PhoneNumber);
                if (checkPhoneNumber == true)
                {
                    return MbResult<ShowBuyerDto>.Failure(new[]
                    {
                        new ErrorDetail("VALIDATION_ERROR", $"Пользователь с таким номером телефона {request.PhoneNumber} уже существует", "EditBuyerCommand")
                    });
                } 
            }

            // Маппинг полей из команды в сущность покупателя
            Buyer updatedBuyer = _mapper.Map(request, buyer);


            // Проверка на уникальность email
            _logger.LogInformation("Проверка уникальности email: {Email}", updatedBuyer.BuyerEmail);
            if (!string.IsNullOrWhiteSpace(updatedBuyer.BuyerEmail))
            {
                bool checkEmail = await _buyerRepository.CheckBuyerEmailAsync(updatedBuyer.BuyerEmail);
                if (checkEmail == true)
                {
                    return MbResult<ShowBuyerDto>.Failure(new[]
                    {
                        new ErrorDetail("VALIDATION_ERROR", $"Пользователь с таким email {updatedBuyer.BuyerEmail} уже существует", "EditBuyerCommand")
                    });
                } 
            }

            try
            {
                // Запрос на обновление покупателя в репозитории
                Buyer updatedResult = await _buyerRepository.UpdateBuyerAsync(updatedBuyer);
                _logger.LogInformation("Покупатель с ID: {BuyerId} успешно обновлен", request.BuyerId);

                // Маппинг обновленного покупателя в DTO для возврата результата
                ShowBuyerDto result = _mapper.Map<ShowBuyerDto>(updatedResult);

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "Buyer", updatedResult.BuyerId, json);

                // Удаление из кэша
                await _cache.RemoveAsync("Buyers:all");
                await _cache.RemoveAsync($"Buyers:{request.BuyerId}");

                _logger.LogInformation("Возврат результата обновления покупателя с ID: {BuyerId}", request.BuyerId);
                return MbResult<ShowBuyerDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении покупателя с ID: {BuyerId}", request.BuyerId);
                return MbResult<ShowBuyerDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "EditBuyerCommand")
                });
            }

        }
    }
}
