using FluentValidation;


namespace CarsTradeAPI.Features.OrdersOperation.OrderItems.EditOrderItem
{
    public class EditOrderItemCommandValidator : FluentValidation.AbstractValidator<EditOrderItemCommand>
    {
        public EditOrderItemCommandValidator() 
        {
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId не должен быть пустым");
            RuleFor(x => x.OrderItemId).NotEmpty().WithMessage("OrderItemId не должен быть пустым");
            RuleFor(x => x.OrderQuantity).GreaterThan(0).WithMessage("OrderQuantity должен быть больше 0");
        }
    }
}
