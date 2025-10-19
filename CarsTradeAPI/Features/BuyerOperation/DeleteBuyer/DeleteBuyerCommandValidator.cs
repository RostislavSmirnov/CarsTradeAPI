using FluentValidation;


namespace CarsTradeAPI.Features.BuyerOperation.DeleteBuyer
{
    public class DeleteBuyerCommandValidator : AbstractValidator<DeleteBuyerCommand>
    {
        public DeleteBuyerCommandValidator()
        {
            RuleFor(command => command.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым");
            RuleFor(command => command.BuyerId).NotEmpty().WithMessage("ID покупателя не должен быть пустым");
        }
    }
}

