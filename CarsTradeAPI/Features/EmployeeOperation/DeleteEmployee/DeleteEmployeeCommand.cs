using MediatR;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.EmployeeOperation.DeleteEmployee
{
    /// <summary>
    /// Класс описывающий команду для удаления сотрудника по его ID
    /// </summary>
    public class DeleteEmployeeCommand : IRequest<MbResult<ShowEmployeeDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public required Guid EmployeeId { get; set; }
    }
}
