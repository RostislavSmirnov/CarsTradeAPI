using MediatR;
using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.BuyerOperation.CreateBuyer
{
    /// <summary>
    /// Класс описывает комманду для создания нового покупателя
    /// </summary>
    public class CreateBuyerCommand : IRequest<MbResult<ShowBuyerDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public required string BuyerName { get; set; }

        public required string BuyerSurname { get; set; }

        public required string BuyerMiddlename { get; set; }

        public required string BuyerEmail { get; set; }

        public required string PhoneNumber { get; set; }

        public required string BuyerAddress { get; set; }
    }
}
