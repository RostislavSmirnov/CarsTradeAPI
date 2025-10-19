using AutoMapper;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Data;
using Microsoft.EntityFrameworkCore;


namespace CarsTradeAPI.Features.EmployeeOperation.EmployeeRepository
{
    /// <summary>
    /// Реализация репозитория для работы с сотрудниками (Employee).
    /// Отвечает за доступ к данным через EF Core и содержит логику CRUD-операций.
    /// </summary>
    public class ImplementationEmployeeRepository : IEmployeeRepository
    {
        private readonly CarsTradeDbContext _carsTradeDbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<ImplementationEmployeeRepository> _logger;
        public ImplementationEmployeeRepository(CarsTradeDbContext carsTradeDbContext, IMapper mapper, ILogger<ImplementationEmployeeRepository> logger)
        {
            _carsTradeDbContext = carsTradeDbContext;
            _mapper = mapper;
            _logger = logger;
        }


        /// <inheritdoc/>
        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            _logger.LogInformation("Этап создания работника с ID {EmployeeId} в репозитории", employee.EmployeeId);
            Employee? employeeResult = await _carsTradeDbContext.Employees.FirstOrDefaultAsync(x => x.EmployeeLogin == employee.EmployeeLogin);
            if (employeeResult != null)
            {
                _logger.LogWarning("Попытка создать пользователя с уже существующим логином: {EmployeeLogin}", employee.EmployeeLogin);
                throw new Exception("Пользователь с таким логином уже существует");
            }
            try
            {
                await _carsTradeDbContext.Employees.AddAsync(employee);
                await _carsTradeDbContext.SaveChangesAsync();
                _logger.LogInformation("Работник успешно создан с ID: {EmployeeId}", employee.EmployeeId);
                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании работника с ID: {EmployeeId}", employee.EmployeeId);
                throw new Exception("Ошибка при создании работника", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<Employee?> DeleteEmployeeAsync(Guid employeeId) 
        {
            _logger.LogInformation("Этап удаления работника с ID {EmployeeId} в репозитории", employeeId);
            // Проверка существования пользователя
            Employee? employeeResult = await _carsTradeDbContext.Employees.FirstOrDefaultAsync(x => x.EmployeeId == employeeId);
            if (employeeResult == null) 
            {
                _logger.LogWarning("Попытка удалить несуществующего пользователя с ID: {EmployeeId}", employeeId);
                return null;
            }
            try
            {
                _carsTradeDbContext.Employees.Remove(employeeResult!);
                await _carsTradeDbContext.SaveChangesAsync();
                _logger.LogInformation("Работник успешно удален с ID: {EmployeeId}", employeeId);
                return employeeResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении работника с ID: {EmployeeId}", employeeId);
                throw new Exception("Ошибка при удалении работника", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<Employee> UpdateEmployeeAsync(Employee employee)
        {
            _logger.LogInformation("Этап редактирования работника с ID {EmployeeId} в репозитории", employee.EmployeeId);
            // Проверка существования пользователя
            Employee? employeeResult = await _carsTradeDbContext.Employees.FirstOrDefaultAsync(x => x.EmployeeId == employee.EmployeeId);
            if (employeeResult == null) 
            {
                _logger.LogWarning("Попытка изменить несуществующего пользователя с ID: {EmployeeId}", employee.EmployeeId);
                throw new Exception($"Пользователь с ID: {employee.EmployeeId} не найден");
            }
            try
            {
                _carsTradeDbContext.Employees.Update(employee);
                await _carsTradeDbContext.SaveChangesAsync();
                _logger.LogInformation("Работник успешно отредактирован с ID: {EmployeeId}", employee.EmployeeId);
                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при редактировании работника с ID: {EmployeeId}", employee.EmployeeId);
                throw new Exception("Ошибка при редактировании работника", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<bool> CheckEmployeeLoginAsync(string employeeLogin) 
        {
            try
            {
                _logger.LogInformation("Этап проверки уникальности логина {EmployeeLogin} в репозитории", employeeLogin);
                bool result = await _carsTradeDbContext.Employees.AnyAsync(x => x.EmployeeLogin == employeeLogin);
                _logger.LogInformation("Проверка уникальности логина {EmployeeLogin} завершена. Результат: {Result}", employeeLogin, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке уникальности логина");
                throw new Exception($"Ошибка при проверке уникальности логина {ex}");
            }
        }


        /// <inheritdoc/>
        public async Task<List<Employee>> GetAllEmployeeAsync()
        {
            _logger.LogInformation("Этап получения всех сотрудников в репозитории");
            try
            {
                List<Employee> employees = await _carsTradeDbContext.Employees.ToListAsync();
                _logger.LogInformation("Все сотрудники успешно получены. Количество: {Count}", employees.Count);
                return employees;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех сотрудников");
                throw new Exception("Ошибка при получении всех сотрудников", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<Employee?> GetEmployeeById(Guid Id) 
        {
            _logger.LogInformation("Этап получения сотрудника по ID {EmployeeId} в репозитории", Id);
            try
            {
                Employee? employee = await _carsTradeDbContext.Employees.FirstOrDefaultAsync(x => x.EmployeeId == Id);
                if (employee == null) 
                {
                    _logger.LogWarning("Попытка получить несуществующего пользователя с ID: {EmployeeId}", Id);
                    return null;
                }
                _logger.LogInformation("Сотрудник успешно получен с ID: {EmployeeId}", Id);
                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении сотрудника с ID: {EmployeeId}", Id);
                throw new Exception("Ошибка при получении сотрудника", ex);
            }
        }


        /// <inheritdoc/>
        public async Task<Employee?> GetEmployeeByLoginAsync(string login)
        {
            _logger.LogInformation("Этап получения сотрудника по логину {EmployeeLogin} в репозитории", login);
            try
            {
                Employee? employee = await _carsTradeDbContext.Employees.FirstOrDefaultAsync(x => x.EmployeeLogin == login);
                if (employee == null) 
                {
                    _logger.LogWarning("Попытка получить несуществующего пользователя с логином: {EmployeeLogin}", login);
                    return null;
                }
                _logger.LogInformation("Сотрудник успешно получен с логином: {EmployeeLogin}", login);
                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении сотрудника с логином: {EmployeeLogin}", login);
                throw new Exception("Ошибка при получении сотрудника", ex);
            }
        }
    }
}
