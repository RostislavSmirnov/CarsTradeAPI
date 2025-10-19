using FluentValidation;

namespace CarsTradeAPI.Features.EmployeeOperation.CreateAdminForTest
{
    public class CreateAdminForTestCommandValidator : AbstractValidator<CreateAdminForTestCommand>
    {
        public CreateAdminForTestCommandValidator() 
        {
            RuleFor(command => command.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым");
            RuleFor(command => command.EmployeeName).NotEmpty().MinimumLength(2);
            RuleFor(command => command.EmployeeSurname).NotEmpty().MinimumLength(2);
            RuleFor(command => command.EmployeeAge).GreaterThan(18).LessThan(100);
            RuleFor(command => command.EmployeeLogin).NotEmpty().MinimumLength(5);
            RuleFor(command => command.EmployeePassword).NotEmpty().MinimumLength(5);
            RuleFor(command => command.EmployeeRole).NotEmpty().Must(role => role == "Admin").WithMessage("Роль должна быть Admin");
        }
    }
}
