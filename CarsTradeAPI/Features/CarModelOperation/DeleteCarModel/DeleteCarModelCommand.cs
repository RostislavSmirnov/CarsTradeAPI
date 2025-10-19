using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;


namespace CarsTradeAPI.Features.CarModelOperation.DeleteCarModel
{
    /// <summary>
    /// Класс описывает команду для удаления модели автомобиля по её уникальному идентификатору (CarModelId).
    /// </summary>
    public class DeleteCarModelCommand : IRequest<MbResult<ShowCarModelDto>>
    {
        public string IdempotencyKey { get; set; } = null!;

        public Guid CarModelId { get; set; }
    }
}
