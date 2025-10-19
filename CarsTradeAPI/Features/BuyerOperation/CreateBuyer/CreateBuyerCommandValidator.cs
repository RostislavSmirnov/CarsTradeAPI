using FluentValidation;


namespace CarsTradeAPI.Features.BuyerOperation.CreateBuyer
{
    public class CreateBuyerCommandValidator : AbstractValidator<CreateBuyerCommand>
    {
        public CreateBuyerCommandValidator()
        {
            RuleFor(command => command.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым");
            RuleFor(command => command.BuyerName).NotEmpty().MinimumLength(2);
            RuleFor(command => command.BuyerSurname).NotEmpty().MinimumLength(2);
            RuleFor(command => command.BuyerMiddlename).NotEmpty().MinimumLength(2);
            RuleFor(command => command.PhoneNumber).NotEmpty().MinimumLength(10).MaximumLength(15);
            RuleFor(command => command.BuyerAddress).NotEmpty().MinimumLength(5);
            RuleFor(command => command.BuyerEmail).EmailAddress().When(command => !string.IsNullOrEmpty(command.BuyerEmail));
        }
    }
}
