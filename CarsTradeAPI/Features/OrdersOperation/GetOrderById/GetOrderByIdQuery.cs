using MediatR;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.OrdersOperation.GetOrderById
{
    /// <summary>
    /// Класс описывающий команду для вывода заказа по его ID
    /// </summary>
    public class GetOrderByIdQuery : IRequest<MbResult<ShowOrderDto>>
    {
        public required Guid Id { get; set; }
    }
}
