namespace CarsTradeAPI.Entities.Elements
{
    /// <summary>
    /// Класс описывающий конфигурацию автомобиля, часть класса CarModel
    /// </summary>
    public class CarConfiguration
    {
        /// <summary>
        /// Свойство указывает на материал интерьера, например кожа, ткань.
        /// </summary>
        public string CarInterior { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на размер колёс
        /// </summary>
        public float WheelSize { get; set; }

        /// <summary>
        /// Свойство указывает на бренд мкзыкальной системы
        /// </summary>
        public string? CarMusicBrand { get; set; } = null!;
    }
}
