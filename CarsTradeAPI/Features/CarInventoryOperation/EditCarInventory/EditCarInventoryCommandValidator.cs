using FluentValidation;


namespace CarsTradeAPI.Features.CarInventoryOperation.EditCarInventory
{
    public class EditCarInventoryCommandValidator : AbstractValidator<EditCarInventoryCommand>
    {
        public EditCarInventoryCommandValidator()
        {
            RuleFor(command => command.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым");
            RuleFor(command => command.Quantity).GreaterThanOrEqualTo(0);
        }
    }
}
