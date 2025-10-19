namespace CarsTradeAPI.Entities.Elements
{
    /// <summary>
    /// Класс описывающий конфигурацию двигателя автомобиля
    /// </summary>
    public class Engine
    {
        /// <summary>
        /// Свойство указывает на объём двигателя
        /// </summary>
        public float EngineCapacity { get; set; }

        /// <summary>
        /// Свойство указывает на тип топлива который использует двигатель: Gasoline, Diesel, Electric, HybridDiesel, HybridGasoline
        /// </summary>
        public string FuelType { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на мощнсоть двигателя в лошидиных силах
        /// </summary>
        public uint EngineHorsePower { get; set; }
    }
}
