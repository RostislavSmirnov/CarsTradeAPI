using FluentValidation;


namespace CarsTradeAPI.Features.OrdersOperation.OrderItems.DeleteOrderItems
{
    public class DeleteOrderItemValidator : AbstractValidator<DeleteOrderItemsCommand>
    {
        public DeleteOrderItemValidator() 
        {
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId не должен быть пустым");
            RuleFor(x => x.OrderItemId).NotEmpty().WithMessage("Список OrderItemId не должен быть пустым");
        }
    }
}
