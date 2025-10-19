using CarsTradeAPI.Entities.Elements;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.CarModelOperation.EditCarModel
{
    /// <summary>
    /// Класс описывающий команду для редактирования информации о модели автомобиля
    /// </summary>
    public class EditCarModelCommand : IRequest<MbResult<ShowCarModelDto>>
    {
        public string IdempotencyKey { get; set; } = null!;
        
        public Guid? CarModelId { get; set; } = null!;

        public string? CarManufacturer { get; set; }

        public string? CarModelName { get; set; }

        public CarConfiguration? CarConfiguration { get; set; }

        public string? CarCountry { get; set; }

        public DateTime? ProductionDateTime { get; set; }

        public Engine? CarEngine { get; set; }

        public string? CarColor { get; set; }

        public decimal? CarPrice { get; set; }
    }
}
