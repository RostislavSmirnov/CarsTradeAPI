using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarsTradeAPI.Entities.Elements;


namespace CarsTradeAPI.Entities
{
    /// <summary>
    /// Класс описывающий автомобиль 
    /// </summary>
    public class CarModel
    {
        /// <summary>
        /// Свойство указывает на ID автомобиля
        /// </summary>
        [Key]
        [Column("car_id")]
        public Guid CarModelId { get; set; }

        /// <summary>
        /// Свойство указывает на марку автомобиля, например: Lada, Lexus
        /// </summary>
        [Column("car_manufacturer")]
        public required string CarManufacturer { get; set; }

        /// <summary>
        /// Свойство указывает на модель автомобиля, например: марка Lada, а модель Vesta 
        /// </summary>
        [Column("car_model")]
        public required string CarModelName { get; set; }

        /// <summary>
        /// Свойство указывает на конфигурацию опций автомобиля, куда входят: салон, размер колёс, музыкальная система
        /// </summary>
        [Column("car_configuration", TypeName = "jsonb")]
        public required CarConfiguration CarConfiguration { get; set; }

        /// <summary>
        /// Свойство указывает на страну-производителя
        /// </summary>
        [Column("car_country")]
        public required string CarCountry { get; set; }
        
        /// <summary>
        /// Свойство указывает на дату производства автомобиля
        /// </summary>
        [Column("car_productionDateTime")]
        public DateTime ProductionDateTime { get; set; }

        /// <summary>
        /// Свойство указывает на параметры двигателя, куда входя: объём двигателя, тип топлива, мощность
        /// </summary>
        [Column("car_engine", TypeName = "jsonb")]
        public Engine? CarEngine { get; set; }

        /// <summary>
        /// Свойство указывает на цвет автомобиля
        /// </summary>
        [Column("car_color")]
        public required string CarColor { get; set; }

        /// <summary>
        /// Свойство указывает на цену автомобиля
        /// </summary>
        [Column("car_price")]
        public decimal CarPrice { get; set; }

        /// <summary>
        /// Свойство указывает на коллекцию элементов заказа
        /// </summary>
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Свойство указывает на коллекцию объектов инвентаря с этим автомобилем
        /// </summary>
        public virtual ICollection<CarInventory> CarInventories { get; set; } = new List<CarInventory>();
    }
}
