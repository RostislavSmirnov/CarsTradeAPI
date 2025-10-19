using MediatR;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.OrdersOperation.OrderItems.EditOrderItem
{
    /// <summary>
    /// Класс описывающий команду для редактирования элемента заказа. 
    /// </summary>
    public class EditOrderItemCommand : IRequest<MbResult<ShowOrderDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public Guid OrderId { get; set; }

        public Guid OrderItemId { get; set; }

        public Guid? CarModelId { get; set; }

        public int? OrderQuantity { get; set; }

        public string? OrderItemComments { get; set; }
    }
}
