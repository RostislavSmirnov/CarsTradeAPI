using CarsTradeAPI.Entities.Elements;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.OrdersOperation.EditOrder
{
    /// <summary>
    /// Класс описывающий команду редактирования заказа
    /// </summary>
    public class EditOrderCommand : IRequest<MbResult<ShowOrderDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public Guid? OrderId { get; set; } = null!;

        public DateTime? OrderCompletionDate { get; set; }

        public OrderAddress? OrderAddress { get; set; }

        public Guid? BuyerId { get; set; }

        public Guid? EmployeeId { get; set; }
    }
}
