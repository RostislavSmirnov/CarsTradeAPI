using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;


namespace CarsTradeAPI.Features.CarInventoryOperation.CreateCarInventory
{
    /// <summary>
    /// Класс описывающий команду для создания нового объекта CarInventory
    /// </summary>
    public class CreateCarInventoryCommand : IRequest<MbResult<ShowCarInventoryDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public required Guid CarModelId { get; set; }

        public required int Quantity { get; set; }
    }
}
