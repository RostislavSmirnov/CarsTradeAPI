namespace CarsTradeAPI.Infrastructure.Services.CarInventoryService
{
    /// <summary>
    /// Интерфейс сервиса для управления инвентарём автомобилей.
    /// Позволяет проверять доступность и изменять количество автомобилей на складе.
    /// </summary>
    public interface ICarInventoryService
    {
        /// <summary>
        /// Проверяет доступность указанного количества автомобилей по ID модели.
        /// </summary>
        /// <param name="carModelId">ID модели автомобиля</param>
        /// <param name="quantity">Запрашиваемое количество</param>
        /// <returns>Результат проверки доступности</returns>
        Task<CarInventoryServiceResult> CheckAvailabilityAsync(Guid carModelId, uint quantity);


        /// <summary>
        /// Уменьшает количество автомобилей в инвентаре.
        /// </summary>
        /// <param name="carModelId">ID модели автомобиля</param>
        /// <param name="quantity">Количество для уменьшения</param>
        Task DecreaseInventoryAsync(Guid carModelId, uint quantity);


        /// <summary>
        /// Увеличивает количество автомобилей в инвентаре.
        /// </summary>
        /// <param name="carModelId">ID модели автомобиля</param>
        /// <param name="quantity">Количество для увеличения</param>
        Task IncreaseInventoryAsync(Guid carModelId, uint quantity);
    }
}
