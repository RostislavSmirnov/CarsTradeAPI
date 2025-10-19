namespace CarsTradeAPI.Infrastructure.Services.CarEngineFuelService
{
    /// <summary>
    /// Интерфейс сервиса для проверки типа топлива.
    /// Позволяет проверять валидность типа топлива.
    /// </summary>
    public interface ICarEngineFuelService
    {
        /// <summary>
        /// Проверяет валидность типа топлива.
        /// </summary>
        /// <param name="fuelTypeInCommand">ID тип топлива указанный в комманде при создании модели авто</param>
        string? CheckCarEngineFuelType(string fuelTypeInCommand);
    }
}
