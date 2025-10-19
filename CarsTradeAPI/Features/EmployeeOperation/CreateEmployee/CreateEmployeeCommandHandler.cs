using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using MediatR;
using CarsTradeAPI.Entities;
using AutoMapper;
using CarsTradeAPI.Features.EmployeeOperation.CreateEmployee;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeRepository;
using CarsTradeAPI.Infrastructure.Services.GenerateTokenService;
using Microsoft.AspNetCore.Identity;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.EmployeeOperation.CreeateEmployee
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, MbResult<ShowEmployeeDto>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly IGenerateToken _generateToken;
        private readonly IPasswordHasher<Employee> _passwordHasher;
        private readonly ILogger<CreateEmployeeCommandHandler> _logger;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ICacheService _cache;
        public CreateEmployeeCommandHandler(
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


        public async Task<MbResult<ShowEmployeeDto>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды CreateEmployeeCommand");

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
                _logger.LogWarning("Попытка создать пользователя с уже существующим логином: {EmployeeLogin}", request.EmployeeLogin);
                return MbResult<ShowEmployeeDto>.Failure(new[]
                {
                    new ErrorDetail("VALIDATION_ERROR", $"Пользователь с таким логином {request.EmployeeLogin} уже существует", "CreateEmployeeCommand")
                });
            }

            try
            {
                // Создание нового сотрудника
                Employee employee = new Employee {EmployeeId = Guid.NewGuid()};
                _mapper.Map(request, employee);
                employee.EmployeePassword = _passwordHasher.HashPassword(employee, employee.EmployeePassword);

                // Сохранение сотрудника в базе данных
                Employee newEmployee = await _employeeRepository.CreateEmployeeAsync(employee);
                ShowEmployeeDto result = _mapper.Map<ShowEmployeeDto>(newEmployee);

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey, "Employee", employee.EmployeeId, json);

                // Удаление из кэша
                await _cache.RemoveAsync("Employee:all");

                _logger.LogInformation("Пользователь успешно создан с ID: {EmployeeId}", newEmployee.EmployeeId);
                return MbResult<ShowEmployeeDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке создать сотрудника с логином: {EmployeeLogin}", request.EmployeeLogin);
                return MbResult<ShowEmployeeDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "CreateEmployeeCommand")
                });
            }
        }
    }
}
