using FluentValidation;


namespace CarsTradeAPI.Features.CarModelOperation.DeleteCarModel
{
    public class DeleteCarModelCommandValidator : AbstractValidator<DeleteCarModelCommand>
    {
        public DeleteCarModelCommandValidator() 
        {
            RuleFor(command => command.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым");
            RuleFor(x => x.CarModelId).NotEmpty().WithMessage("Id для удаления автомобиля не может быть пустым");
        }
    }
}
