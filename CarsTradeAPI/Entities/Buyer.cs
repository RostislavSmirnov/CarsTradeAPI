using System.ComponentModel.DataAnnotations.Schema;


namespace CarsTradeAPI.Entities
{
    /// <summary>
    /// Класс описывающий сущность клиента(покупателя)
    /// </summary>
    public class Buyer
    {
        /// <summary>
        /// Свойство указывает на ID покупателя
        /// </summary>
        [Column("buyer_id")]
        public Guid BuyerId { get; set; }

        /// <summary>
        /// Свойство указывает на имя покупателя
        /// </summary>
        [Column("buyer_name")]
        public string BuyerName { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на фамилию покупателя
        /// </summary>
        [Column("buyer_surname")]
        public string BuyerSurname { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на отчество покупателя
        /// </summary>
        [Column("buyer_middlename")]
        public string BuyerMiddlename { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на электронную почту покупателя 
        /// </summary>
        [Column("buyer_email")]
        public string BuyerEmail { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на дату создания сущности покупателя
        /// </summary>
        [Column("buyer_created_date")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Свойство указывает на номер телефона покупателя 
        /// </summary>
        [Column("phone_number")]
        public string PhoneNumber { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на адресс покупателя
        /// </summary>
        [Column("buyer_adress")]
        public string BuyerAddress { get; set; } = null!;

        /// <summary>
        /// Свойство указывает на коллекцию заказов
        /// </summary>
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    } 
}