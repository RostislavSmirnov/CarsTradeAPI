using FluentValidation;
namespace CarsTradeAPI.Features.CarInventoryOperation.CreateCarInventory
{
    public class CreateCarInventoryCommandValidator : AbstractValidator<CreateCarInventoryCommand>
    {
        public CreateCarInventoryCommandValidator()
        {
            RuleFor(x => x.CarModelId)
                .NotEmpty().WithMessage("Идентификатор модели автомобиля не может быть пустым.");
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Количество должно быть больше нуля.");
            RuleFor(command => command.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым");
        }
    }
}
