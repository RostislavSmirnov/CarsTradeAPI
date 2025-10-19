using FluentValidation;


namespace CarsTradeAPI.Features.OrdersOperation.OrderItems.AddOrderItem
{
    public class CreateOrderItemCommandValidator : AbstractValidator<CreateOrderItemCommand>
    {
        public CreateOrderItemCommandValidator() 
        {
            RuleFor(oi => oi.CarModelId).NotEmpty().WithMessage("ID модели автомобиля не должен быть пустым");
            RuleFor(oi => oi.OrderQuantity).GreaterThan(0).WithMessage("Количество автомобилей в заказе должно быть больше нуля");
            RuleFor(oi => oi.OrderItemComments).MaximumLength(500).WithMessage("Комментарий не должен превышать 500 символов");
        }
    }
}
