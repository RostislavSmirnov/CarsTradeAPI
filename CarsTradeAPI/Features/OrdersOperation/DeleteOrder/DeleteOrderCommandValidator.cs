using FluentValidation;


namespace CarsTradeAPI.Features.OrdersOperation.DeleteOrder
{
    public class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
    {
        public DeleteOrderCommandValidator()
        {
            RuleFor(command => command.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым");
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("ID заказа не должен быть пустым");
        }
    }
}
