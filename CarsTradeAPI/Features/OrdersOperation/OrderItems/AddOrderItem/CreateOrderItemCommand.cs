using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.OrdersOperation.OrderItems.AddOrderItem
{
    /// <summary>
    /// Класс описывающий комманду для добавления элемента заказа
    /// </summary>
    public class CreateOrderItemCommand : IRequest <MbResult<ShowOrderDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public required Guid OrderId { get; set; }

        public required Guid CarModelId { get; set; }

        public required int OrderQuantity { get; set; }

        public string? OrderItemComments { get; set; }
    }
}
