using CarsTradeAPI.Infrastructure.Services.CarEngineFuelService;
using FluentValidation;


namespace CarsTradeAPI.Features.CarModelOperation.CreateCarModel
{
    public class CreateCarModelCommandValidator: AbstractValidator<CreateCarModelCommand>
    {
        public CreateCarModelCommandValidator()
        {
            RuleFor(command => command.IdempotencyKey).NotEmpty().WithMessage("Ключ идемпотентности не может быть пустым");
            RuleFor(x => x.CarModelName)
                .NotEmpty().WithMessage("Модель автомобиля не может быть пустым.");
            RuleFor(x => x.CarConfiguration)
                .NotEmpty().WithMessage("Конфигурация автомобиля не может быть пустой.");
            RuleFor(x => x.CarEngine)
                .NotEmpty().WithMessage("Информация о двигателе не может быть пустой");
            RuleFor(x => x.CarEngine!.FuelType)
                .NotEmpty().WithMessage("Тип топлива не может быть пустым")
                .IsEnumName(typeof(FuelTypeEnum), caseSensitive: false)
                .WithMessage("Указан недопустимый тип топлива.");
            RuleFor(x => x.CarColor).NotEmpty().WithMessage("Информация о цвете автомобиле не может быть пустой");
            RuleFor(x => x.CarManufacturer).NotEmpty().WithMessage("Информация о марке автомобиля не может быть пустой");
            RuleFor(x => x.CarCountry).NotEmpty().WithMessage("Информация о стране-производителе автомобиле не может быть пустой");
            RuleFor(x => x.CarPrice).GreaterThan(0).NotEmpty().WithMessage("Информация о цене автомобиля не может быть пустой");
        }
    }
}
