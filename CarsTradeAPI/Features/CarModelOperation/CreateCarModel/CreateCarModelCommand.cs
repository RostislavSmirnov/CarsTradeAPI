using CarsTradeAPI.Entities.Elements;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;


namespace CarsTradeAPI.Features.CarModelOperation.CreateCarModel
{
    /// <summary>
    /// Касс описывающий команду для создания новой модели автомобиля
    /// </summary>
    public class CreateCarModelCommand : IRequest<MbResult<ShowCarModelDto>> 
    {
        public string IdempotencyKey { get; set; } = null!;

        public required string CarManufacturer { get; set; }

        public required string CarModelName { get; set; }

        public required CarConfiguration CarConfiguration { get; set; }

        public required string CarCountry { get; set; }

        public required DateTime ProductionDateTime { get; set; }

        public required Engine? CarEngine { get; set; }

        public required string CarColor { get; set; }

        public required decimal CarPrice { get; set; }
    }
}
