using CarsTradeAPI.Entities.Elements;


namespace CarsTradeAPI.Features.CarModelOperation.CarModelDto
{
    /// <summary>
    /// DTO для отображения информации о модели автомобиля
    /// </summary>
    public class ShowCarModelDto
    {
        public Guid CarModelId { get; set; }

        public string CarManufacturer { get; set; } = null!;

        public string CarModelName { get; set; } = null!;

        public CarConfiguration CarConfiguration { get; set; } = null!;

        public string CarCountry { get; set; } = null!;

        public DateTime ProductionDateTime { get; set; }

        public Engine? CarEngine { get; set; }

        public string CarColor { get; set; } = null!;

        public decimal CarPrice { get; set; }
    }
}
