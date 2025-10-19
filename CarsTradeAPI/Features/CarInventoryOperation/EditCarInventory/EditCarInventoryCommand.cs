using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.CarInventoryOperation.EditCarInventory
{
    /// <summary>
    /// Класс описывающий команду для редактирования существующего объекта CarInventory
    /// </summary>
    public class EditCarInventoryCommand : IRequest<MbResult<ShowCarInventoryDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public Guid InventoryId { get; set; }

        public int? Quantity { get; set; }
    }
}
