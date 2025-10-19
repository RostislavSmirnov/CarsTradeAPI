using FluentValidation;


namespace CarsTradeAPI.Features.EmployeeOperation.EmployeeAuthentication
{
    public class EmployeeAuthenticationCommandValidator : AbstractValidator<EmployeeAuthenticationCommand>
    {
        public EmployeeAuthenticationCommandValidator()
        {
            RuleFor(x => x.Login).NotEmpty().WithMessage("Логин не должен быть пустым");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Пароль не должен быть пустым");
        }
    }
}
