namespace CarsTradeAPI.Infrastructure.ValidationBehavior
{
    /// <summary>
    /// Интерфейс, представляющий результат выполнения операции.
    /// </summary>
    public interface IResult
    {
        /// <summary>
        /// Признак успешного выполнения операции.
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// Коллекция ошибок, если операция завершилась с ошибками.
        /// </summary>
        IReadOnlyList<ErrorDetail> Errors { get; }
    }

    /// <summary>
    /// Класс, представляющий детализированную информацию об ошибке.
    /// </summary>
    /// <param name="Code">Код ошибки</param>
    /// <param name="Message">Сообщение об ошибке</param>
    /// <param name="Field">Поле или объект, к которому относится ошибка</param>
    public record ErrorDetail(string Code, string Message, string Field)
    {
        /// <summary>
        /// Фабричный метод для создания ошибки валидации.
        /// </summary>
        /// <param name="field">Поле, вызвавшее ошибку</param>
        /// <param name="message">Сообщение об ошибке</param>
        /// <returns>Объект <see cref="ErrorDetail"/> с кодом VALIDATION_ERROR</returns>
        public static ErrorDetail Validation(string field, string message) =>
            new ErrorDetail("VALIDATION_ERROR", message ?? string.Empty, field ?? string.Empty);
    }
}