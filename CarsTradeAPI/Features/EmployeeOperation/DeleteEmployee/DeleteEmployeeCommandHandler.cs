using MediatR;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeRepository;
using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using System.Text.Json;
using CarsTradeAPI.Infrastructure.Services.CacheService;


namespace CarsTradeAPI.Features.EmployeeOperation.DeleteEmployee
{
    public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, MbResult<ShowEmployeeDto>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DeleteEmployeeCommandHandler> _logger;
        private readonly IIdempotencyService _IdempotencyService;
        private readonly ICacheService _cache;
        public DeleteEmployeeCommandHandler(
            IEmployeeRepository employeeRepository,
            IMapper mapper, ILogger<DeleteEmployeeCommandHandler> logger,
            IIdempotencyService idempotencyService,
            ICacheService cacheService)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _logger = logger;
            _IdempotencyService = idempotencyService;
            _cache = cacheService;
        }


        public async Task<MbResult<ShowEmployeeDto>> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Начало обработки команды DeleteEmployeeCommand");

            // Проверка ключа Идемпотентности 
            (Guid? resourceId, string? responseJson) = await _IdempotencyService.GetByKeyAsync(request.IdempotencyKey);
            if (resourceId.HasValue)
            {
                Employee? existingEmployee = await _employeeRepository.GetEmployeeById(resourceId.Value);
                ShowEmployeeDto dto = _mapper.Map<ShowEmployeeDto>(existingEmployee);
                return MbResult<ShowEmployeeDto>.Success(dto);
            }


            try
            {
                // Удаление пользователя из репозитория
                Employee? deletedEmployee = await _employeeRepository.DeleteEmployeeAsync(request.EmployeeId);
                if (deletedEmployee == null)
                {
                    _logger.LogWarning("Попытка удалить несуществующего пользователя с ID: {EmployeeId}", request.EmployeeId);
                    return MbResult<ShowEmployeeDto>.Failure(new[]
                    {
                        new ErrorDetail("NOT_FOUND", $"Пользователь с ID: {request.EmployeeId} не найден", "DeleteEmployeeCommand")
                    });
                }
                _logger.LogInformation("Пользователь с ID: {EmployeeId} успешно удален", request.EmployeeId);
                
                // Возврат результата, c преобразованием в ShowEmployeeDto
                ShowEmployeeDto result = _mapper.Map<ShowEmployeeDto>(deletedEmployee);

                // Сохранение ключа идемпотентности
                string json = JsonSerializer.Serialize(result);
                await _IdempotencyService.SaveAsync(request.IdempotencyKey, "Employee", deletedEmployee.EmployeeId, json);

                // Удаление из кэша
                await _cache.RemoveAsync($"Employee:{request.EmployeeId}");
                await _cache.RemoveAsync("Employee:all");

                return MbResult<ShowEmployeeDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке удалить пользователя с ID: {EmployeeId}", request.EmployeeId);
                return MbResult<ShowEmployeeDto>.Failure(new[]
                {
                    new ErrorDetail("Exception", ex.Message, "DeleteEmployeeCommand")
                });
            }          
        }
    }
}
