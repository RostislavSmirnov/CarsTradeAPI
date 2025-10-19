using MediatR;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.EmployeeOperation.GetEmployeeById
{
    /// <summary>
    /// Класс описывающий запрос на получение сотрудника по ID
    /// </summary>
    public class GetEmployeeByIdQuery : IRequest<MbResult<ShowEmployeeDto>>
    {
        public required Guid EmployeeId { get; set; }
    }
}

