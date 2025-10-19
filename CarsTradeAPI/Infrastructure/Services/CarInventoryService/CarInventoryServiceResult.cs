namespace CarsTradeAPI.Infrastructure.Services.CarInventoryService
{
    /// <summary>
    /// Результат проверки наличия автомобиля на складе
    /// </summary>
    public class CarInventoryServiceResult
    {
        public bool IsAvailable { get; set; }
        public uint AvailableQuantity { get; set; }
    }
}
