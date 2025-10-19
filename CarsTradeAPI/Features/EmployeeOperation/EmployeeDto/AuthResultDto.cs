namespace CarsTradeAPI.Features.EmployeeOperation.EmployeeDto
{
    /// <summary>
    /// Класс описывающий DTO для результата аутентификации сотрудника
    /// </summary>
    public class AuthResultDto
    {
        public required string Token { get; set; }
        public DateTime Expiration { get; set; }
        public required string Role { get; set; }
    }
}
