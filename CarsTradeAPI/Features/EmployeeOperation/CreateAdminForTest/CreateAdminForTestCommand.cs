using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using MediatR;


namespace CarsTradeAPI.Features.EmployeeOperation.CreateAdminForTest
{
    /// <summary>
    /// Команда  создания нового администратора для тестов
    /// </summary>
    public class CreateAdminForTestCommand : IRequest<MbResult<ShowEmployeeDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public required string EmployeeName { get; set; }

        public required string EmployeeSurname { get; set; }

        public required string? EmployeeMiddlename { get; set; }

        public required int EmployeeAge { get; set; }

        public required string EmployeeLogin { get; set; }

        public required string EmployeePassword { get; set; }

        public string EmployeeRole { get; set; } = "Admin"; // По умолчанию
    }
}
