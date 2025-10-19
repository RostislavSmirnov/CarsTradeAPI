using MediatR;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.OrdersOperation.GetAllOrders
{
    // Класс описывающий команду для вывода всех заказов
    public class GetAllOrdersQuery : IRequest<MbResult<List<ShowOrderDto>>>
    {
        
    }
}
