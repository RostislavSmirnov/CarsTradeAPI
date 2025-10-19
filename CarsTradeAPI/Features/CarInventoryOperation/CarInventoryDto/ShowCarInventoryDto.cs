using CarsTradeAPI.Features.CarModelOperation.CarModelDto;


namespace CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto
{
    /// <summary>
    /// Класс описывает DTO для показа информации о объекте инвентаря автомобилей
    /// </summary>
    public class ShowCarInventoryDto
    {
        public Guid InventoryId { get; set; }

        public Guid CarModelId { get; set; }

        public required ShowCarModelDto CarModel { get; set; }

        public uint Quantity { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
