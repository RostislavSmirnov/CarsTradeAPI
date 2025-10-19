using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace CarsTradeAPI.Entities
{
    /// <summary>
    /// Класс описывающий сущнсость элемента заказа 
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// Свойство указывает на ID элемента заказа
        /// </summary>
        [Key]
        [Column("orderitem_id")]
        public Guid OrderItemId { get; set; }

        /// <summary>
        /// Свойство указывает на количество экземпляров элемента заказа
        /// </summary>
        [Column("order_quantity")]
        public long OrderQuantity { get; set; }

        /// <summary>
        /// Свойство указывает на цену за одну единицу элемента заказа
        /// </summary>
        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Свойство указывает на комментарий к элементу заказа
        /// </summary>
        [Column("comments")]
        public string? OrderItemComments { get; set; }

        /// <summary>
        /// Свойство указывает на ID заказа, к которому ссылается элемент заказа
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// Навигационное свойство EF
        /// </summary>
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на модель автомобиля, к которому ссылается элемент заказа
        /// </summary>
        public Guid CarModelId { get; set; }

        /// <summary>
        /// Навигационное свойство EF
        /// </summary>
        [ForeignKey("CarModelId")]
        public virtual CarModel CarModel { get; set; } = null!;
    }
}