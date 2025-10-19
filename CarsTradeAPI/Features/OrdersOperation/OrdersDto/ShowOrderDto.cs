using CarsTradeAPI.Entities.Elements;


namespace CarsTradeAPI.Features.OrdersOperation.OrdersDto
{
    /// <summary>
    /// Класс описывающий DTO для отображения информации о заказе
    /// </summary>
    public class ShowOrderDto
    {
        public Guid OrderId { get; set; }

        public DateTime OrderCreateDate { get; set; }

        public DateTime? OrderCompletionDate { get; set; }

        public decimal OrderPrice { get; set; }

        public OrderAddress OrderAddress { get; set; } = null!;

        public Guid BuyerId { get; set; }

        public Guid EmployeeId { get; set; }

        public List<ShowOrderItemDto> OrderItems { get; set; } = new();
    }
}
