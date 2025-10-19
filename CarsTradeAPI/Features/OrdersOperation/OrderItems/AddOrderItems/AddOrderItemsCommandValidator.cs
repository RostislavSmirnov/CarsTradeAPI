using FluentValidation;


namespace CarsTradeAPI.Features.OrdersOperation.OrderItems.AddOrderItems
{
    public class AddOrderItemsCommandValidator : AbstractValidator<AddOrderItemsCommand>
    {
        public AddOrderItemsCommandValidator()
        {
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("ID заказа не должен быть пустым");
            
            RuleForEach(x => x.OrderItems).ChildRules(orderItem =>
            {
                orderItem.RuleFor(oi => oi.CarModelId).NotEmpty().WithMessage("ID модели автомобиля не должен быть пустым");
                orderItem.RuleFor(oi => oi.OrderQuantity).GreaterThan(0).WithMessage("Количество автомобилей в заказе должно быть больше нуля");
                orderItem.RuleFor(oi => oi.OrderItemComments).MaximumLength(500).WithMessage("Комментарий не должен превышать 500 символов");
            });
        }
    }
}
