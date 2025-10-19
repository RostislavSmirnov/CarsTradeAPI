using MediatR;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.EmployeeOperation.EditEmployee
{
    /// <summary>
    /// Класс описывающий команду для реадактирования информации о сотруднике 
    /// </summary>
    public class EditEmployeeCommand : IRequest<MbResult<ShowEmployeeDto>>
    {
        public string? IdempotencyKey { get; set; }

        public Guid? EmployeeId { get; set; }

        public string? EmployeeName { get; set; }

        public string? EmployeeSurname { get; set; }

        public string? EmployeeMiddlename { get; set; }

        public int? EmployeeAge { get; set; }

        public int? EmployeeSellCounter { get; set; }

        public string? EmployeeLogin { get; set; }

        public string? EmployeePassword { get; set; }

        public string? EmployeeRole { get; set; }
    }
}
