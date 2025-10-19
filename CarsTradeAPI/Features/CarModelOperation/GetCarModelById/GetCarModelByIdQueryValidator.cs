using FluentValidation;


namespace CarsTradeAPI.Features.CarModelOperation.GetCarModelById
{
    public class GetCarModelByIdQueryValidator : AbstractValidator<GetCarModelByIdQuery>
    {
        public GetCarModelByIdQueryValidator()
        {
            RuleFor(x => x.CarModelId).NotEmpty().WithMessage("ID модели автомобиля не должен быть пустым");
        }
    }
}
