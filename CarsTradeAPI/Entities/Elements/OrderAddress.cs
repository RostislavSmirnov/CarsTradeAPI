namespace CarsTradeAPI.Entities.Elements
{
    /// <summary>
    /// Класс описывающий объект адресса, куда может быть доставка авто
    /// </summary>
    public class OrderAddress
    {
        /// <summary>
        /// Сойство указывает на страну
        /// </summary>
        public string Country { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на регион
        /// </summary>
        public string Region { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на город
        /// </summary>
        public string City { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на улицу и дом
        /// </summary>
        public string Street { get; set; } = null!;
    }
}
