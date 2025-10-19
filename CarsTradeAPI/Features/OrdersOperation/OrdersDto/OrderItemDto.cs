namespace CarsTradeAPI.Features.OrdersOperation.OrdersDto
{
    /// <summary>
    /// Класс описывающий элемент заказа
    /// </summary>
    public class OrderItemDto
    {
        public int OrderQuantity { get; set; }

        public string? OrderItemComments { get; set; }

        public Guid CarModelId { get; set; }
    }
}
