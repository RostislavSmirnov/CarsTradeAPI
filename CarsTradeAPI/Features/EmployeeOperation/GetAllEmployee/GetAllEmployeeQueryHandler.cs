using MediatR;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeRepository;
using AutoMapper;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.EmployeeOperation.GetAllEmployee
{
    public class GetAllEmployeeQueryHandler : IRequestHandler<GetAllEmployeeQuery, MbResult<List<ShowEmployeeDto>>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllEmployeeQueryHandler> _logger;
        private readonly ICacheService _cache;
        public GetAllEmployeeQueryHandler(IEmployeeRepository employeeRepository, IMapper mapper, ILogger<GetAllEmployeeQueryHandler> logger, ICacheService cache)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }


        public async Task<MbResult<List<ShowEmployeeDto>>> Handle(GetAllEmployeeQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды GetAllEmployeeQuery");

            // Проверка кэша
            string cacheKey = "Employee:all";
            List<ShowEmployeeDto>? cached = await _cache.GetAsync<List<ShowEmployeeDto>>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Получение {Count} сотрудников из кэша", cached.Count);
                return MbResult<List<ShowEmployeeDto>>.Success(cached);
            }

            try
            {
                // Получение всех сотрудников из репозитория, преобразование в ShowEmployeeDto и возврат результата
                List<Employee> allEmployees = await _employeeRepository.GetAllEmployeeAsync();
                List<ShowEmployeeDto> result =_mapper.Map<List<ShowEmployeeDto>>(allEmployees);

                // Сохранение в кэш
                await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Успешное получение всех сотрудников. Количество: {Count}", result.Count);
                return MbResult<List<ShowEmployeeDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке получить всех сотрудников");
                return MbResult<List<ShowEmployeeDto>>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "GetAllEmployeeQuery")
                });
            }
        }
    }
}
