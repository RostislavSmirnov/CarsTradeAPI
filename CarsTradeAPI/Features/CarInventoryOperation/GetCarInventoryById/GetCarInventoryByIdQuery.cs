using MediatR;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.CarInventoryOperation.GetCarInventoryById
{
    /// <summary>
    /// Класс описывающий запрос для получения информации о объекте инвенторя по его ID
    /// </summary>
    public class GetCarInventoryByIdQuery : IRequest<MbResult<ShowCarInventoryDto>>
    {
        public required Guid CarInventoryId { get; set; }
    }
}
