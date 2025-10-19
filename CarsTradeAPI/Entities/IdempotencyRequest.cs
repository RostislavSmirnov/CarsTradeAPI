using System.ComponentModel.DataAnnotations;


namespace CarsTradeAPI.Entities
{
    /// <summary>
    /// Класс описывающий сущность модели для реализации идемпотентности
    /// </summary>
    public class IdempotencyRequest
    {
        /// <summary>
        /// Свойство указывает на ID модели для реализации идемпотентности
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Свойство указывает на ключ идемпотентности 
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Свойство указывает на статус записи идемпотентности
        /// </summary>
        [MaxLength(50)]
        public string Status { get; set; } = "Completed";

        /// <summary>
        /// Свойство указывает на дату создания записи идемпотентности
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Свойство указывает на тип ресурса, к которому будет применяться идемпотентность
        /// </summary>
        [MaxLength(50)]
        public string ResourceType { get; set; } = string.Empty;

        /// <summary>
        /// Свойство указывает на ID ресурса которому будет применяться идемпотентность
        /// </summary>
        public Guid? ResourceId { get; set; }

        /// <summary>
        /// Свойство указывает на сериализованный ответ чтобы моментально делать возврат
        /// </summary>
        public string? Responsejson { get; set; }
    }
}
