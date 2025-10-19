using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.CarModelOperation.GetCarModelById
{
    /// <summary>
    /// Класс описывает запрос для получения автомобиля по ID
    /// </summary>
    public class GetCarModelByIdQuery:IRequest<MbResult<ShowCarModelDto>>
    {
        public Guid CarModelId { get; set; }
    }
}
