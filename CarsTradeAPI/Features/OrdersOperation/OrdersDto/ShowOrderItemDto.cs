namespace CarsTradeAPI.Features.OrdersOperation.OrdersDto
{
    /// <summary>
    /// Класс описывающий DTO для отображения информации о позиции заказа
    /// </summary>
    public class ShowOrderItemDto 
    {
        public Guid OrderItemId { get; set; }
       
        public Guid OrderId { get; set; }

        public uint OrderQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        public string? OrderItemComments { get; set; }
    }
}
