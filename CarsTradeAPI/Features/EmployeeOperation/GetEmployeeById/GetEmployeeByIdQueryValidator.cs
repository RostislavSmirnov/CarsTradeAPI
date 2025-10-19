using FluentValidation;


namespace CarsTradeAPI.Features.EmployeeOperation.GetEmployeeById
{
    public class GetEmployeeByIdQueryValidator : AbstractValidator<GetEmployeeByIdQuery>
    {
        public GetEmployeeByIdQueryValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("ID сотрудника не должен быть пустым");
        }
    }
}
