using FluentValidation;


namespace CarsTradeAPI.Features.CarInventoryOperation.GetCarInventoryById
{
    public class GetCarInventoryByIdQueryValidator : AbstractValidator<GetCarInventoryByIdQuery>
    {
        public GetCarInventoryByIdQueryValidator()
        {
            RuleFor(x => x.CarInventoryId).NotEmpty().WithMessage("ID автомобиля не должен быть пустым");
        }
    }
}
