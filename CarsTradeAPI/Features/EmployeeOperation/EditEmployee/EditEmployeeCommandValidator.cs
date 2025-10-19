using FluentValidation;

namespace CarsTradeAPI.Features.EmployeeOperation.EditEmployee
{
    public class EditEmployeeCommandValidator : AbstractValidator<EditEmployeeCommand>
    {
        public EditEmployeeCommandValidator() 
        {
            RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Id сотрудника не может быть пустым");
            RuleFor(x => x.EmployeeAge).GreaterThan(18).WithMessage("Возраст сотрудника должен быть больше 18 лет");
            RuleFor(x => x.EmployeeSellCounter).GreaterThanOrEqualTo(0).WithMessage("Количество продаж не может быть отрицательным");
            RuleFor(x => x.EmployeeName).MinimumLength(2);
            RuleFor(x => x.EmployeeSurname).MinimumLength(2);
            RuleFor(x => x.EmployeeLogin).MinimumLength(5);
            RuleFor(x => x.EmployeePassword).MinimumLength(5);
            RuleFor(command => command.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым");
        }
    }
}
