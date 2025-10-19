using FluentValidation;


namespace CarsTradeAPI.Features.BuyerOperation.GetBuyerById
{
    public class GetBuyerQueryValidator : AbstractValidator<GetBuyerByIdQuery>
    {
        public GetBuyerQueryValidator()
        {
            RuleFor(x => x.BuyerId).NotEmpty().WithMessage("ID покупателя не должен быть пустым");
        }
    }
}
