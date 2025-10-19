namespace CarsTradeAPI.Infrastructure.Services.GenerateTokenService
{
    /// <summary>
    /// Интерфейс сервиса генерации JWT токенов.
    /// </summary>
    public interface IGenerateToken
    {
        /// <summary>
        /// Генерирует JWT токен на основе данных сотрудника.
        /// </summary>
        /// <param name="employee">Сотрудник, для которого создаётся токен</param>
        /// <returns>Сгенерированный JWT токен</returns>
        string GenerateToken(Entities.Employee employee);
    }
}
