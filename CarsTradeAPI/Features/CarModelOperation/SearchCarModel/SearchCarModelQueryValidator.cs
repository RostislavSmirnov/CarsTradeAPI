using CarsTradeAPI.Infrastructure.Services.CarEngineFuelService;
using FluentValidation;


namespace CarsTradeAPI.Features.CarModelOperation.SearchCarModel
{
    public class SearchCarModelQueryValidator : AbstractValidator<SearchCarModelQuery>
    {
        public SearchCarModelQueryValidator() 
        {
            RuleFor(x => x.CarEngine!.FuelType).IsEnumName(typeof(FuelTypeEnum), caseSensitive: false).WithMessage("Указан недопустимый тип топлива.");
            RuleFor(x => x.CarPrice).GreaterThan(0).WithMessage("Информация о цене автомобиля не может быть пустой");
        }
    }
}
