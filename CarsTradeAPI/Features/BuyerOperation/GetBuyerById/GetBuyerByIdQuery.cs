using MediatR;
using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.BuyerOperation.GetBuyerById
{
    /// <summary>
    /// Класс описывающий запрос для получения информации о покупателе по его ID
    /// </summary>
    public class GetBuyerByIdQuery : IRequest<MbResult<ShowBuyerDto>>
    {
        public Guid BuyerId { get; set; }
    }
}
