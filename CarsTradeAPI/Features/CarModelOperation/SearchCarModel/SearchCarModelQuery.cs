using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Entities.Elements;


namespace CarsTradeAPI.Features.CarModelOperation.SearchCarModel
{
    /// <summary>
    /// Класс описывающий запрос для поиска машин
    /// </summary>
    public class SearchCarModelQuery : IRequest<MbResult<List<ShowCarModelDto>>>
    {
        public Guid? CarModelId { get; set; }

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
