using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.OrdersOperation.OrderItems.DeleteOrderItems
{
    // Класс описыввающий команду для удаления элементов заказа 
    public class DeleteOrderItemsCommand : IRequest<MbResult<ShowOrderDto>>
    {
        public required string IdempotencyKey { get; set; }

        public Guid OrderId { get; set; }

        public List<Guid> OrderItemId { get; set; } = new();
    }
}
