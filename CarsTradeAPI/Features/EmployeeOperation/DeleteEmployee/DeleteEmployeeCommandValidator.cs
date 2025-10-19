using FluentValidation;


namespace CarsTradeAPI.Features.EmployeeOperation.DeleteEmployee
{
    public class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
    {
        public DeleteEmployeeCommandValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("ID сотрудника не должен быть пустым");
            RuleFor(command => command.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым");
        }
    }
}
