using MediatR;
using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.BuyerOperation.DeleteBuyer
{
    /// <summary>
    /// Класс описывающий команду для удаления покупателя по его ID
    /// </summary>
    public class DeleteBuyerCommand : IRequest<MbResult<ShowBuyerDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public required Guid BuyerId { get; set; }
    }
}
