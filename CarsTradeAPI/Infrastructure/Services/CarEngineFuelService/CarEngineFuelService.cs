namespace CarsTradeAPI.Infrastructure.Services.CarEngineFuelService
{
    /// <summary>
    /// Сервис для проверки типа топлива двигателя автомобиля
    /// </summary>
    public class CarEngineFuelService : ICarEngineFuelService
    {
        /// <inheritdoc/>
        public string? CheckCarEngineFuelType(string fuelTypeInCommand)
        {
            if (Enum.TryParse<FuelTypeEnum>(fuelTypeInCommand ,true, out var fuelType))
            {
                return fuelType.ToString().ToLower();
            }
            return null;
        }
    }
}
