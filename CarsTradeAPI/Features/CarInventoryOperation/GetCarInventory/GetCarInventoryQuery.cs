using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.CarInventoryOperation.GetCarInventory
{
    /// <summary>
    /// Класс описывающий запрос для получения списка всех объектов CarInventory
    /// </summary>
    public class GetCarInventoryQuery : IRequest<MbResult<List<ShowCarInventoryDto>>>
    {
    }
}
