using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.OrdersOperation.DeleteOrder
{
    /// <summary>
    /// Класс описывающий запрос на удаление заказа по ID
    /// </summary>
    public class DeleteOrderCommand : IRequest<MbResult<ShowOrderDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public Guid OrderId { get; set; }
    }
}
