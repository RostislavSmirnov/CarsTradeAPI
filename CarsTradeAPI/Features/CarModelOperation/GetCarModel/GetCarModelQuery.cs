using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.CarModelOperation.GetCarModel
{
    /// <summary>
    /// Класс описывает запрос для получения всех моделей автомобилей
    /// </summary>
    public class GetCarModelQuery : IRequest<MbResult<List<ShowCarModelDto>>>
    {

    }
}
