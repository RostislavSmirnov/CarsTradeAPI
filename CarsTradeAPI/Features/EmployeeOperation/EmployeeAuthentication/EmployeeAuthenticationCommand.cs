using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.EmployeeOperation.EmployeeAuthentication
{
    /// <summary>
    /// Класс описывающий команду аутентификации сотрудника
    /// </summary>
    public class EmployeeAuthenticationCommand : IRequest<MbResult<AuthResultDto>>
    {
        public required string Login { get; set; }

        public required string Password { get; set; }
    }
}
