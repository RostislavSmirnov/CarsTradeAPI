using MediatR;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Entities;
using AutoMapper;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeRepository;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using Microsoft.AspNetCore.Identity;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.EmployeeOperation.EditEmployee
{
    public class EditEmployeeCommandHandler : IRequestHandler<EditEmployeeCommand, MbResult<ShowEmployeeDto>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EditEmployeeCommandHandler> _logger;
        private readonly IPasswordHasher<Employee> _passwordHasher;
        private readonly IIdempotencyService _idempotencyService;
        private readonly ICacheService _cache;
        public EditEmployeeCommandHandler(
            IEmployeeRepository employeeRepository,
            IMapper mapper, ILogger<EditEmployeeCommandHandler> logger,
            IPasswordHasher<Employee> passwordHasher,
            IIdempotencyService idempotencyService,
            ICacheService cacheService)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _logger = logger;
            _passwordHasher = passwordHasher;
            _idempotencyService = idempotencyService;
            _cache = cacheService;
        }


        public async Task<MbResult<ShowEmployeeDto>> Handle(EditEmployeeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды EditEmployeeCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _idempotencyService.GetByKeyAsync(request.IdempotencyKey!);
            if (resourceId.HasValue)
            {
                Employee? existingEmployee = await _employeeRepository.GetEmployeeById(resourceId.Value);
                ShowEmployeeDto dto = _mapper.Map<ShowEmployeeDto>(existingEmployee);
                return MbResult<ShowEmployeeDto>.Success(dto);
            }

            // Проверка на уникальность логина
            if (request.EmployeeLogin != null)
            {
                bool result = await _employeeRepository.CheckEmployeeLoginAsync(request.EmployeeLogin);
                if (result == true)
                {
                    _logger.LogWarning("Попытка изменить пользователя с уже существующим логином: {EmployeeLogin}", request.EmployeeLogin);
                    return MbResult<ShowEmployeeDto>.Failure(new[]
                    {
                        new ErrorDetail("VALIDATION_ERROR", $"Пользователь с таким логином {request.EmployeeLogin} уже существует", "EditEmployeeCommand")
                    });
                }
            }

            // Проверка существования пользователя
            Employee? employee = await _employeeRepository.GetEmployeeById((Guid)request.EmployeeId!);          
            if (employee == null)
            {
                _logger.LogWarning("Попытка изменить несуществующего пользователя с ID: {EmployeeId}", request.EmployeeId);
                return MbResult<ShowEmployeeDto>.Failure(new[]
                {
                    new ErrorDetail("NOT_FOUND", $"Пользователь с ID: {request.EmployeeId} не найден", "EditEmployeeCommand")
                });
            }

            try
            {
                Employee editedEmployee = _mapper.Map(request, employee);
                // Если пароль был передан, хэшируем его
                if (!string.IsNullOrWhiteSpace(request.EmployeePassword))
                {
                    _logger.LogInformation("Хэширование нового пароля для пользователя с ID: {EmployeeId}", request.EmployeeId);
                    editedEmployee.EmployeePassword = _passwordHasher.HashPassword(editedEmployee, request.EmployeePassword);
                }

                Employee updatedEmployee = await _employeeRepository.UpdateEmployeeAsync(editedEmployee);
                _logger.LogInformation("Пользователь с ID: {EmployeeId} успешно обновлен", request.EmployeeId);
                ShowEmployeeDto result = _mapper.Map<ShowEmployeeDto>(updatedEmployee);

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _idempotencyService.SaveAsync(request.IdempotencyKey!, "Employee", employee.EmployeeId, json);

                // Удаление из кэша
                await _cache.RemoveAsync($"Employee:{request.EmployeeId}");
                await _cache.RemoveAsync("Employee:all");

                _logger.LogInformation("Пользователь успешно обновлён с ID: {EmployeeId}", updatedEmployee.EmployeeId);
                return MbResult<ShowEmployeeDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке обновить пользователя с ID: {EmployeeId}", request.EmployeeId);
                return MbResult<ShowEmployeeDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "EditEmployeeCommand")
                });
            }
        }
    }
}
