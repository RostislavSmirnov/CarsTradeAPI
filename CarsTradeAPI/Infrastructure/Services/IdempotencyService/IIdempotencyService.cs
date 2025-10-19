namespace CarsTradeAPI.Infrastructure.Services.IdempotencyService
{
    /// <summary>
    /// Интерфейс сервиса идемпотентности.
    /// Используется для предотвращения повторной обработки одинаковых запросов.
    /// </summary>
    public interface IIdempotencyService
    {
        /// <summary>
        /// Получает сохранённый результат операции по ключу идемпотентности.
        /// </summary>
        /// <param name="key">Ключ идемпотентности</param>
        /// <returns>Кортеж: ID ресурса и сериализованный ответ, или null, если не найден</returns>
        Task<(Guid? resourceId, string? responseJson)> GetByKeyAsync(string key);

        /// <summary>
        /// Сохраняет результат операции для обеспечения идемпотентности.
        /// </summary>
        /// <param name="key">Ключ идемпотентности</param>
        /// <param name="resourceType">Тип ресурса</param>
        /// <param name="resourceId">ID созданного или изменённого ресурса</param>
        /// <param name="responseJson">Сериализованный ответ (опционально)</param>
        Task SaveAsync(string key, string resourceType, Guid resourceId, string? responseJson = null);
    }
}
