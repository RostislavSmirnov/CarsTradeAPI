namespace CarsTradeAPI.Features.EmployeeOperation.EmployeeDto
{
    public class ShowEmployeeDto
    {
        public required Guid EmployeeId { get; set; }

        public required string EmployeeName { get; set; }

        public required string EmployeeSurname { get; set; }

        public required string EmployeeMiddlename { get; set; }

        public required int EmployeeAge { get; set; }

        public required uint EmployeeSellCounter { get; set; }

        public required string EmployeeRole { get; set; }
    }
}
