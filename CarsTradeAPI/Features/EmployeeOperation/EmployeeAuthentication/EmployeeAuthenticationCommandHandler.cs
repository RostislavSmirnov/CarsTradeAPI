using MediatR;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Entities;
using AutoMapper;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeRepository;
using CarsTradeAPI.Infrastructure.Services.GenerateTokenService;
using Microsoft.AspNetCore.Identity;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;


namespace CarsTradeAPI.Features.EmployeeOperation.EmployeeAuthentication
{
    public class EmployeeAuthenticationCommandHandler : IRequestHandler<EmployeeAuthenticationCommand, MbResult<AuthResultDto>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly IGenerateToken _generateToken;
        private readonly IPasswordHasher<Employee> _passwordHasher;
        private readonly ILogger<EmployeeAuthenticationCommandHandler> _logger;
        private readonly IIdempotencyService _IdempotencyService;
        public EmployeeAuthenticationCommandHandler(
            IEmployeeRepository employeeRepository,
            IMapper mapper, IGenerateToken generateToken,
            IPasswordHasher<Employee> passwordHasher,
            ILogger<EmployeeAuthenticationCommandHandler> logger,
            IIdempotencyService idempotencyService)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _generateToken = generateToken;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _IdempotencyService = idempotencyService;
        }


        public async Task<MbResult<AuthResultDto>> Handle(EmployeeAuthenticationCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды EmployeeAuthenticationCommand");

            // Проверка существования пользователя
            Employee? employee = await _employeeRepository.GetEmployeeByLoginAsync(request.Login);
            if (employee == null)
            {
                _logger.LogWarning("Попытка входа с несуществующим логином: {Login}", request.Login);
                return MbResult<AuthResultDto>.Failure(new[]
                {
                    new ErrorDetail("NOT_FOUND", $"Пользователь с логином {request.Login} не найден", "EmployeeAuthenticationCommand")
                });
            }

            try
            {
                // Проверка пароля
                PasswordVerificationResult passwordResult = _passwordHasher.VerifyHashedPassword(employee, employee.EmployeePassword, request.Password);
                if (passwordResult == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Попытка входа с неверным паролем для логина: {Login}", request.Login);
                    return MbResult<AuthResultDto>.Failure(new[]
                    {
                        new ErrorDetail("VALIDATION_ERROR", "Неверный пароль", "EmployeeAuthenticationCommand")
                    });
                }

                // Генерация JWT токена
                string token = _generateToken.GenerateToken(employee);
                AuthResultDto authResult = new AuthResultDto
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddHours(1),
                    Role = employee.EmployeeRole
                };
                _logger.LogInformation("Пользователь с логином: {Login} успешно аутентифицирован", request.Login);
                return MbResult<AuthResultDto>.Success(authResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке аутентификации пользователя с логином: {Login}", request.Login);
                return MbResult<AuthResultDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "EmployeeAuthenticationCommand")
                });
            }
        }
    }
}
