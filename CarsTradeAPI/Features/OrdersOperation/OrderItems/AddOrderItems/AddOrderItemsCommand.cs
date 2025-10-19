using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.OrdersOperation.OrderItems.AddOrderItems
{
    /// <summary>
    /// Класс описывающий команду для добавления нескольуих элементов заказа в заказ
    /// </summary>
    public class AddOrderItemsCommand : IRequest<MbResult<ShowOrderDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public required Guid OrderId { get; set; }

        public required List<OrderItemDto> OrderItems { get; set; } = new();
    }
}
