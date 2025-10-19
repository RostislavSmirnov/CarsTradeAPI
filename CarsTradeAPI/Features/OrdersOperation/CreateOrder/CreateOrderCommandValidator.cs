using FluentValidation;


namespace CarsTradeAPI.Features.OrdersOperation.CreateOrder
{
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator() 
        {
            RuleFor(command => command.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым");
            RuleFor(x => x.BuyerId).NotEmpty().WithMessage("ID покупателя не должен быть пустым");
            RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("ID сотрудника не должен быть пустым");
            RuleFor(x => x.OrderAddress).NotEmpty().WithMessage("Адресс не должен быть пустым");
            RuleFor(x => x.OrderCompletionDate).GreaterThan(DateTime.Now).When(x => x.OrderCompletionDate != null).WithMessage("Дата завершения заказа не может быть меньше текущей даты");
            RuleFor(x => x.OrderCompletionDate).NotNull().WithMessage("Дата завершения заказа не должна быть пустой");
        }
    }
}
