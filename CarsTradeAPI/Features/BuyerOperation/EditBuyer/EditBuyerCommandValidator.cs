using FluentValidation;


namespace CarsTradeAPI.Features.BuyerOperation.EditBuyer
{
    public class EditBuyerCommandValidator : AbstractValidator<EditBuyerCommand>
    {
        public EditBuyerCommandValidator() 
        {
            RuleFor(command => command.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым");
            RuleFor(command => command.PhoneNumber).Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Неверный формат номера телефона");
            RuleFor(command => command.BuyerEmail).EmailAddress().When(command => !string.IsNullOrEmpty(command.BuyerEmail)).WithMessage("Неверный формат email адреса");
        }
    }
}
