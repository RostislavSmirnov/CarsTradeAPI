using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.CarModelOperation.SearchCarModel;

namespace CarsTradeAPI.Features.CarModelOperation.CarModelRepository
{
    /// <summary>
    /// Репозиторий для работы с моделями автомобилей
    /// Определяет контракт для CRUD-операций и поиска
    /// </summary>
    public interface ICarModelRepository
    {
        /// <summary>
        /// Создает новую модель автомобиля в системе
        /// </summary>
        /// <param name="carModel">Экземпляр модели автомобиля для создания</param>
        /// <returns>Созданная модель автомобиля с присвоенным ID</returns>
        Task<CarModel> CreateCarModelAsync(CarModel carModel);


        /// <summary>
        /// Удаляет модель автомобиля по идентификатору
        /// </summary>
        /// <param name="CarModelId">UUID модели автомобиля</param>
        /// <returns>Удалённая модель или null, если не найдена</returns>
        Task<CarModel?> DeleteCarModelAsync(Guid CarModelId);


        /// <summary>
        /// Обновляет данные существующей модели автомобиля
        /// </summary>
        /// <param name="carModel">Обновлённые данные модели</param>
        /// <returns>Обновлённая модель автомобиля</returns>
        Task<CarModel> EditCarModelAsync(CarModel carModel);


        /// <summary>
        /// Получает список всех моделей автомобилей
        /// </summary>
        /// <returns>Коллекция моделей автомобилей</returns>
        Task<List<CarModel>> GetCarModelsAsync();


        /// <summary>
        /// Получает модель автомобиля по идентификатору
        /// </summary>
        /// <param name="CarModelId">UUID модели автомобиля</param>
        /// <returns>Модель автомобиля или null, если не найдена</returns>
        Task<CarModel?> GetCarModelByIdAsync(Guid CarModelId);


        /// <summary>
        /// Выполняет поиск моделей автомобилей по заданным критериям
        /// </summary>
        /// <param name="query">Параметры поиска</param>
        /// <returns>Коллекция найденных моделей</returns>
        Task<List<CarModel>> SearchCarModels(SearchCarModelQuery query);
    }
}
