using CarsTradeAPI.Entities;


namespace CarsTradeAPI.Features.EmployeeOperation.EmployeeRepository
{
    /// <summary>
    /// Репозиторий для работы с сущностью Employee.
    /// Определяет контракт для CRUD-операций и проверок, связанных с сотрудниками.
    /// </summary>
    public interface IEmployeeRepository
    {
        /// <summary>
        /// Создаёт нового сотрудника в системе.
        /// </summary>
        /// <param name="command">Данные сотрудника для создания</param>
        /// <returns>Созданный сотрудник с заполненным EmployeeId</returns>
        /// <exception cref="Exception">При ошибке сохранения в БД или если логин уже занят</exception>
        Task<Employee> CreateEmployeeAsync(Employee command);


        /// <summary>
        /// Удаляет сотрудника.
        /// </summary>
        /// <param name="employeeId"> уникальный ID сотрудника  EmployeeId)</param>
        /// <returns>Удалённый сотрудник или null, если не найден</returns>
        /// <exception cref="Exception">При ошибке удаления</exception>
        Task<Employee?> DeleteEmployeeAsync(Guid employeeId);


        /// <summary>
        /// Обновляет данные существующего сотрудника.
        /// </summary>
        /// <param name="command">Обновлённые данные сотрудника</param>
        /// <returns>Обновлённый сотрудник</returns>
        /// <exception cref="Exception">При ошибке сохранения в БД или если сотрудник не найден</exception>
        Task<Employee> UpdateEmployeeAsync(Employee command);


        /// <summary>
        /// Проверяет, существует ли сотрудник с указанным логином.
        /// </summary>
        /// <param name="employeeLogin">Логин сотрудника для проверки</param>
        /// <returns>True если логин занят, иначе False</returns>
        /// <exception cref="Exception">При ошибке выполнения запроса</exception>
        Task<bool> CheckEmployeeLoginAsync(string employeeLogin);


        /// <summary>
        /// Получает всех сотрудников из системы.
        /// </summary>
        /// <returns>Список всех сотрудников</returns>
        /// <exception cref="Exception">При ошибке выборки</exception>
        Task<List<Employee>> GetAllEmployeeAsync();


        /// <summary>
        /// Получает сотрудника по идентификатору.
        /// </summary>
        /// <param name="Id">UUID сотрудника</param>
        /// <returns>Найденный сотрудник или null</returns>
        /// <exception cref="Exception">При ошибке выполнения запроса</exception>
        Task<Employee?> GetEmployeeById(Guid Id);


        /// <summary>
        /// Получает сотрудника по логину.
        /// </summary>
        /// <param name="login">Логин сотрудника</param>
        /// <returns>Найденный сотрудник или null</returns>
        /// <exception cref="Exception">При ошибке выполнения запроса</exception>
        Task<Employee?> GetEmployeeByLoginAsync(string login);
    }
}