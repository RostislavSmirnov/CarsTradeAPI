using CarsTradeAPI.Entities;


namespace CarsTradeAPI.Features.CarInventoryOperation.CarInventoryRepository
{
    /// <summary>
    /// Репозиторий для работы с объектами инвентаря автомобилей
    /// Обеспечивает абстракцию над EF Core для выполнения CRUD-операций
    /// </summary>
    public interface ICarInventoryRepository
    {
        /// <summary>
        /// Создает новый объект CarInventory
        /// </summary>
        /// <param name="command">Данные инвентаря для создания</param>
        /// <returns>Созданный объект CarInventory</returns>
        Task<CarInventory> CreateCarInventoryAsync(CarInventory command);


        /// <summary>
        /// Удаляет объект CarInventory по его идентификатору
        /// </summary>
        /// <param name="carInventoryId">UUID объекта инвентаря</param>
        /// <returns>Удаленный объект или null, если он не найден</returns>
        /// <exception cref="ArgumentException">Если объект не существует</exception>
        Task<CarInventory?> DeleteCarInventoryAsync(Guid carInventoryId);


        /// <summary>
        /// Получает список всех объектов CarInventory
        /// </summary>
        /// <returns>Список объектов CarInventory</returns>
        Task<List<CarInventory>> GetAllCarInventoriesAsync();


        /// <summary>
        /// Получает объект CarInventory по идентификатору
        /// </summary>
        /// <param name="carInventoryId">UUID объекта инвентаря</param>
        /// <returns>Найденный объект CarInventory или null, если он отсутствует</returns>
        Task<CarInventory?> GetCarInventoryByIdAsync(Guid carInventoryId);


        /// <summary>
        /// Обновляет данные существующего объекта CarInventory
        /// </summary>
        /// <param name="carInventory">Обновленные данные объекта</param>
        /// <returns>Обновленный объект CarInventory</returns>
        /// <exception cref="ArgumentException">Если объект не существует</exception>
        Task<CarInventory> EditCarInventoryAsync(CarInventory carInventory);


        /// <summary>
        /// Получает модель автомобиля по идентификатору
        /// </summary>
        /// <param name="CarModelId">UUID модели автомобиля</param>
        /// <returns>Найденная модель автомобиля или null</returns>
        Task<CarModel?> GetCarModelByIdAsync(Guid CarModelId);
    }
}
