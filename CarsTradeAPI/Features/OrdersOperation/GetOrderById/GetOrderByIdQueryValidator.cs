using FluentValidation;


namespace CarsTradeAPI.Features.OrdersOperation.GetOrderById
{
    public class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
    {
        public GetOrderByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("ID заказа не должен быть пустым");
        }
    }
}
