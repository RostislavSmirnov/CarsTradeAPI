using CarsTradeAPI.Infrastructure.Services.CarEngineFuelService;
using FluentValidation;


namespace CarsTradeAPI.Features.CarModelOperation.EditCarModel
{
    public class EditCarModelCommandValidator : AbstractValidator<EditCarModelCommand>
    {
        public EditCarModelCommandValidator() 
        {
            RuleFor(x => x.CarModelId).NotEmpty().WithMessage("ID автомобиля для редактирования не может быть пустым");
            RuleFor(x => x.CarManufacturer);
            RuleFor(x => x.CarConfiguration);
            RuleFor(x => x.CarEngine!.FuelType)
                .IsEnumName(typeof(FuelTypeEnum), caseSensitive: false)
                .WithMessage("Указан недопустимый тип топлива.");
            RuleFor(x => x.ProductionDateTime).LessThan(DateTime.Now).WithMessage("Дата производства не может быть в будущем");
            RuleFor(x => x.CarPrice).GreaterThan(0).WithMessage("Цена автомобиля должна быть больше нуля");
            RuleFor(command => command.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым");
        }
    }
}
