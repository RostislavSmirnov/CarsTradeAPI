using FluentValidation;


namespace CarsTradeAPI.Features.OrdersOperation.EditOrder
{
    public class EditOrderCommandValidator : AbstractValidator<EditOrderCommand>
    {
        public EditOrderCommandValidator() 
        {
            RuleFor(command => command.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым");
            RuleFor(x => x.OrderId).NotEmpty().WithMessage("Id не может быть пустым");
            RuleFor(x => x.OrderCompletionDate).GreaterThan(DateTime.Now).When(x => x.OrderCompletionDate != null).WithMessage("Дата завершения заказа не может быть меньше текущей даты");
        }
    }
}
