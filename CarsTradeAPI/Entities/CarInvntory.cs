using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CarsTradeAPI.Entities
{
    /// <summary>
    /// Класс описывающий объект инвентаря, по своей сути это список модели автомобиля в наличии.
    /// </summary>
    public class CarInventory
    {
        /// <summary>
        /// Свойство указывает на ID объекта инвентаря
        /// </summary>
        [Key]
        [Column("inventory_id")]
        public Guid InventoryId { get; set; }

        /// <summary>
        /// Свойство указывает на ID автомобиля, наличие которого описывает этот класс
        /// </summary>
        [Column("car_model_id")]
        public Guid CarModelId { get; set; }

        /// <summary>
        ///  Свойство указывает на количество автомобилей в наличии
        /// </summary>
        [Column("quantity")]
        public uint Quantity { get; set; }

        /// <summary>
        /// Свойство указывает на последние изменения в объекте инвентаря
        /// </summary>
        [Column("last_updated")]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Навигационное свойство для EF
        /// </summary>
        [ForeignKey("CarModelId")]
        public virtual CarModel CarModel { get; set; } = null!;
    }
}
