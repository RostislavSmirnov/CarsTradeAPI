using MediatR;
using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.BuyerOperation.GetBuyers
{
    /// <summary>
    /// Класс описывающий запрос для получения списка всех покупателей
    /// </summary>
    public class GetBuyersQuery : IRequest<MbResult<List<ShowBuyerDto>>>
    {
    }
}
