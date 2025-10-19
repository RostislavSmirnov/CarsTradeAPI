using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.CarInventoryOperation.DeleteCarInventory
{
    /// <summary>
    /// Класс описывающий команду для удаления объекта CarInventory по его идентификатору
    /// </summary>
    public class DeleteCarInventoryCommand : IRequest<MbResult<ShowCarInventoryDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public required Guid CarInventoryId { get; set; }
    }
}
