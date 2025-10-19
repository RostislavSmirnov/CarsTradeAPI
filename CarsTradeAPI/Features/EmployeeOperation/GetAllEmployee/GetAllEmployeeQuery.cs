using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.EmployeeOperation.GetAllEmployee
{
    /// <summary>
    /// Класс описывающий запрос на получение всех сотрудников
    /// </summary>
    public class GetAllEmployeeQuery : IRequest<MbResult<List<ShowEmployeeDto>>>
    {
    }
}
