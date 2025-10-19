namespace CarsTradeAPI.Infrastructure.Services.CacheService
{
    /// <summary>
    /// Интерфейс сервиса для работы с кэшем.
    /// Позволяет получать, сохранять и удалять данные по ключу.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Получает значение из кэша по заданному ключу.
        /// </summary>
        /// <typeparam name="T">Тип сохраняемого значения</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <returns>Значение из кэша или null, если ключ отсутствует</returns>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Сохраняет значение в кэш с возможностью задания времени жизни.
        /// </summary>
        /// <typeparam name="T">Тип сохраняемого значения</typeparam>
        /// <param name="key">Ключ кэша</param>
        /// <param name="value">Значение для сохранения</param>
        /// <param name="absoluteExpireTime">Абсолютное время жизни (удаляется после указанного времени)</param>
        /// <param name="slidingExpireTime">Скользящее время жизни (обновляется при каждом обращении)</param>
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpireTime = null, TimeSpan? slidingExpireTime = null);

        /// <summary>
        /// Удаляет значение из кэша по ключу.
        /// </summary>
        /// <param name="key">Ключ кэша</param>
        Task RemoveAsync(string key);
    }
}
