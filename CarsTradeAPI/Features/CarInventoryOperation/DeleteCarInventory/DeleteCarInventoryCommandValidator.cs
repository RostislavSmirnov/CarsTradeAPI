using FluentValidation;


namespace CarsTradeAPI.Features.CarInventoryOperation.DeleteCarInventory
{
    public class DeleteCarInventoryCommandValidator : AbstractValidator<DeleteCarInventoryCommand>
    {
        public DeleteCarInventoryCommandValidator()
        {
            RuleFor(x => x.CarInventoryId).NotEmpty().WithMessage("ID записи об автомобиле не должен быть пустым");
            RuleFor(command => command.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым");
        }
    }
}
