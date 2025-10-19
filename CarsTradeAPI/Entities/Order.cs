using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CarsTradeAPI.Entities.Elements;


namespace CarsTradeAPI.Entities
{
    /// <summary>
    /// Класс описывающий сущность заказа 
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Свойство указывает на ID заказа
        /// </summary>
        [Key]
        [Column("order_id")]
        public Guid OrderId { get; set; }

        /// <summary>
        /// Свойство указывает на дату создания заказа
        /// </summary>
        [Column("order_create_date")]
        public DateTime OrderCreateDate { get; set; }

        /// <summary>
        /// Свойство указывает на дату прибытия заказа
        /// </summary>
        [Column("order_completion_date")]
        public DateTime? OrderCompletionDate { get; set; }

        /// <summary>
        /// Свойство указывает на общую сумму заказа, складывается из OrderItem
        /// </summary>
        [Column("order_price")]
        public decimal OrderPrice { get; set; }

        /// <summary>
        /// Свойство указывает на адресс, куда должен придти заказ
        /// </summary>
        [Column("order_delivery_address", TypeName = "jsonb")]
        public OrderAddress OrderAddress { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на ID покупателя, для которого делается заказ
        /// </summary>
        public Guid BuyerId { get; set; }
       
        /// <summary>
        /// Навигационное свойство EF
        /// </summary>
        [ForeignKey("BuyerId")]
        public virtual Buyer Buyer { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на работника, который осуществил заказ 
        /// </summary>
        public Guid EmployeeId { get; set; }

        /// <summary>
        /// Навигационное свойство EF
        /// </summary>
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на коллекцию элементов заказа 
        /// </summary>
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}