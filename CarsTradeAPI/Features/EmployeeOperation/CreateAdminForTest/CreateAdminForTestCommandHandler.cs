using MediatR;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.EmployeeOperation.CreeateEmployee;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeRepository;
using CarsTradeAPI.Infrastructure.Services.CacheService;
using CarsTradeAPI.Infrastructure.Services.GenerateTokenService;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;


namespace CarsTradeAPI.Features.EmployeeOperation.CreateAdminForTest
{
    public class CreateAdminForTestCommandHandler : IRequestHandler<CreateAdminForTestCommand, MbResult<ShowEmployeeDto>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly IGenerateToken _generateToken;
        private readonly IPasswordHasher<Employee> _passwordHasher;
        private readonly ILogger<CreateEmployeeCommandHandler> _logger;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ICacheService _cache;
        public CreateAdminForTestCommandHandler(
            IEmployeeRepository employeeRepository,
            IMapper mapper, IGenerateToken generateToken,
            IPasswordHasher<Employee> passwordHasher,
            ILogger<CreateEmployeeCommandHandler> logger,
            IIdempotencyService idempotencyService,
            ICacheService cacheService)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _generateToken = generateToken;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _idempotencyService = idempotencyService;
            _cache = cacheService;
        }


        public async Task<MbResult<ShowEmployeeDto>> Handle(CreateAdminForTestCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды CreateAdminForTestCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                Employee? existingEmployee = await _employeeRepository.GetEmployeeById(resourceId.Value);
                ShowEmployeeDto dto = _mapper.Map<ShowEmployeeDto>(existingEmployee);
                return MbResult<ShowEmployeeDto>.Success(dto);
            }

            // Проверка на уникальность логина
            bool loginResult = await _employeeRepository.CheckEmployeeLoginAsync(request.EmployeeLogin);
            if (loginResult == true)
            {
                _logger.LogWarning("Попытка создать админа с уже существующим логином: {EmployeeLogin}", request.EmployeeLogin);
                return MbResult<ShowEmployeeDto>.Failure(new[]
                {
                    new ErrorDetail("VALIDATION_ERROR", $"Админ с таким логином {request.EmployeeLogin} уже существует", "CreateEmployeeCommand")
                });
            }

            try
            {
                // Создание нового админа для тестирования
                Employee employee = new Employee { EmployeeId = Guid.NewGuid() };
                _mapper.Map(request, employee);
                employee.EmployeePassword = _passwordHasher.HashPassword(employee, employee.EmployeePassword);

                // Сохранение админа в базе данных
                Employee newEmployee = await _employeeRepository.CreateEmployeeAsync(employee);
                ShowEmployeeDto result = _mapper.Map<ShowEmployeeDto>(newEmployee);

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "Employee", employee.EmployeeId, json);

                // Удаление из кэша
                await _cache.RemoveAsync("Employee:all");

                _logger.LogInformation("Администратор успешно создан с ID: {EmployeeId}", newEmployee.EmployeeId);
                return MbResult<ShowEmployeeDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке создать админа с логином: {EmployeeLogin}", request.EmployeeLogin);
                return MbResult<ShowEmployeeDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "CreateEmployeeCommand")
                });
            }
        }
    }
}
