using MediatR;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Entities.Elements;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.OrdersOperation.CreateOrder
{
    /// <summary>
    /// Класс описывающий команду создания заказа
    /// </summary>
    public class CreateOrderCommand : IRequest<MbResult<ShowOrderDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public required OrderAddress OrderAddress { get; set; }
        
        public required DateTime? OrderCompletionDate { get; set; }

        public required Guid BuyerId { get; set; }

        public required Guid EmployeeId { get; set; }

        public List<OrderItemDto> OrderItems { get; set; } = new();
    }
}


