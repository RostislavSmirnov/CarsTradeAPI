using MediatR;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.EmployeeOperation.CreateEmployee
{
    /// <summary>
    /// Команда описывающая команду создание нового сотрудника
    /// </summary>
    public class CreateEmployeeCommand : IRequest<MbResult<ShowEmployeeDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public required string EmployeeName { get; set; }

        public required string EmployeeSurname { get; set; }

        public required string? EmployeeMiddlename { get; set; }

        public required int EmployeeAge { get; set; }

        public required string EmployeeLogin { get; set; }

        public required string EmployeePassword { get; set; }

        public string EmployeeRole { get; set; } = "User"; // По умолчанию
    }
}
