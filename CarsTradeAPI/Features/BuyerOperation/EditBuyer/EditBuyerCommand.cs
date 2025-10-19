using MediatR;
using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.BuyerOperation.EditBuyer
{
    /// <summary>
    /// Класс описывающий команду для редактирования информации о покупателе
    /// </summary>
    public class EditBuyerCommand : IRequest<MbResult<ShowBuyerDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public required Guid BuyerId { get; set; }

        public string? BuyerName { get; set; }

        public string? BuyerSurname { get; set; }

        public string? BuyerMiddlename { get; set; }

        public string? BuyerEmail { get; set; }

        public string? PhoneNumber { get; set; }

        public string? BuyerAddress { get; set; }
    }
}
