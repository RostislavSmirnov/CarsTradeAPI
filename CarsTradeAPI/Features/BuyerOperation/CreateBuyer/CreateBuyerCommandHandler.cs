using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using MediatR;
using CarsTradeAPI.Features.BuyerOperation.BuyerRepository;
using CarsTradeAPI.Entities;
using AutoMapper;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.BuyerOperation.CreateBuyer
{
    public class CreateBuyerCommandHandler : IRequestHandler<CreateBuyerCommand, MbResult<ShowBuyerDto>>
    {
        private readonly IBuyerRepository _buyerRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateBuyerCommandHandler> _logger;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ICacheService _cache;
        public CreateBuyerCommandHandler(
            IBuyerRepository buyerRepository,
            IMapper mapper,
            ILogger<CreateBuyerCommandHandler> logger,
            IIdempotencyService idempotencyService,
            ICacheService cacheService)
        {
            _buyerRepository = buyerRepository;
            _mapper = mapper;
            _logger = logger;
            _idempotencyService = idempotencyService;
            _cache = cacheService;
        }


        public async Task<MbResult<ShowBuyerDto>> Handle(CreateBuyerCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды CreateBuyerCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                Buyer? existingBuyer = await _buyerRepository.GetBuyerByIdAsync(resourceId.Value);
                ShowBuyerDto dto = _mapper.Map<ShowBuyerDto>(existingBuyer);
                return MbResult<ShowBuyerDto>.Success(dto);
            }

            // Проверка на уникальность номера телефона и email
            _logger.LogInformation("Проверка уникальности номера телефона: {PhoneNumber}", request.PhoneNumber);
            bool checkPhoneNumber = await _buyerRepository.CheckBuyerPhoneNumberAsync(request.PhoneNumber);
            if (checkPhoneNumber == true)
            {
                _logger.LogWarning("Попытка создать пользователя с уже существующим номером телефона: {PhoneNumber}", request.PhoneNumber);
                return MbResult<ShowBuyerDto>.Failure(new[]
                {
                    new ErrorDetail("NOT_FOUND", $"Пользователь с таким номером телефона {request.PhoneNumber} уже существует", "CreateBuyerCommand")
                });
            }
            
            // Проверка на уникальность email
            _logger.LogInformation("Проверка уникальности email: {Email}", request.BuyerEmail);
            bool checkEmail = await _buyerRepository.CheckBuyerEmailAsync(request.BuyerEmail);
            if (checkEmail == true)
            {
                _logger.LogWarning("Попытка создать пользователя с уже существующим email: {BuyerEmail}", request.BuyerEmail);
                return MbResult<ShowBuyerDto>.Failure(new[]
                {
                    new ErrorDetail("NOT_FOUND", $"Пользователь с таким email {request.BuyerEmail} уже существует", "CreateBuyerCommand")
                });
            }

            // Создание нового покупателя
            try
            {
                // Создание нового покупателя
                Buyer buyer = new Buyer { BuyerId = Guid.NewGuid() };
                buyer.CreatedAt = DateTime.UtcNow;
                _mapper.Map(request, buyer);

                // Сохранение покупателя в базе данных
                Buyer newBuyer = await _buyerRepository.CreateBuyerAsync(buyer);
                _logger.LogInformation("Пользователь успешно создан с ID: {BuyerId}", newBuyer.BuyerId);

                // Возврат результата, c преобразованием в ShowBuyerDto
                ShowBuyerDto result = _mapper.Map<ShowBuyerDto>(newBuyer);

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "Buyer", newBuyer.BuyerId, json);

                // Удаление из кэша
                await _cache.RemoveAsync("Buyers:all");

                _logger.LogInformation("Завершение обработки команды CreateBuyerCommand");
                return MbResult<ShowBuyerDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании нового покупателя");
                return MbResult<ShowBuyerDto>.Failure(new[]
                {
                    new ErrorDetail("Server", ex.Message, "CreateBuyerCommand")
                });
            }
        }
    }
}
