using MediatR;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using AutoMapper;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeRepository;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.EmployeeOperation.GetEmployeeById
{
    public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, MbResult<ShowEmployeeDto>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetEmployeeByIdQueryHandler> _logger;
        private readonly ICacheService _cache;
        public GetEmployeeByIdQueryHandler(
            IEmployeeRepository employeeRepository,
            IMapper mapper,
            ILogger<GetEmployeeByIdQueryHandler> logger,
            ICacheService cacheService)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _logger = logger;
            _cache = cacheService;
        }


        public async Task<MbResult<ShowEmployeeDto>> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды GetEmployeeByIdQuery");

            // Проверка кэша
            string cacheKey = $"Employee:{request.EmployeeId}";
            ShowEmployeeDto? cached = await _cache.GetAsync<ShowEmployeeDto>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Получение сотрудника по ID: {EmployeeId} из кэша", request.EmployeeId);
                return MbResult<ShowEmployeeDto>.Success(cached);
            }

            // Проверка существования сотрудника
            Employee? employee = await _employeeRepository.GetEmployeeById(request.EmployeeId);
            if (employee == null)
            {
                _logger.LogWarning("Попытка получить несуществующего пользователя с ID: {EmployeeId}", request.EmployeeId);
                return MbResult<ShowEmployeeDto>.Failure(new[]
                {
                    new ErrorDetail("NOT_FOUND", $"Пользователь с ID: {request.EmployeeId} не найден", "GetEmployeeByIdQuery")
                });
            }
            try
            {
                ShowEmployeeDto result = _mapper.Map<ShowEmployeeDto>(employee);

                // Сохранение в кэш
                await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Пользователь с ID: {EmployeeId} успешно найден", request.EmployeeId);
                return MbResult<ShowEmployeeDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке получить пользователя с ID: {EmployeeId}", request.EmployeeId);
                return MbResult<ShowEmployeeDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "GetEmployeeByIdQuery")
                });
            }
        }
    }
}
