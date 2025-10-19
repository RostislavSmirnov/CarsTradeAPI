namespace CarsTradeAPI.Infrastructure.ValidationBehavior
{
    /// <summary>
    /// Параметризованный класс результата операции с возможностью возврата значения или ошибок
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MbResult<T> : IResult
    {
        // Указывает, был ли результат успешным
        public bool IsSuccess { get; private set; }

        // Значение результата, если операция была успешной
        public T? Value { get; private set; }

        // Список ошибок, если таковые имеются
        public IReadOnlyList<ErrorDetail> Errors { get; private set; } = Array.Empty<ErrorDetail>();

        // Приватный конструктор для предотвращения прямого создания экземпляров
        private MbResult() { }

        // Статический метод для создания успешного результата
        public static MbResult<T> Success(T value) => new MbResult<T> { IsSuccess = true, Value = value };

        // Статический метод для создания неуспешного результата с ошибками
        public static MbResult<T> Failure(IEnumerable<ErrorDetail> errors) => new MbResult<T> { IsSuccess = false, Errors = errors.ToList()};
    }

    // Непараметризованный вариант MbResult для случаев, когда не нужно возвращать значение
    public class MbResult : IResult
    {
        public bool IsSuccess { get; private set; }
        public IReadOnlyList<ErrorDetail> Errors { get; private set; } = Array.Empty<ErrorDetail>();

        private MbResult() { }

        public static MbResult Success() => new MbResult { IsSuccess = true };

        public static MbResult Failure(IEnumerable<ErrorDetail> errors) =>
            new MbResult { IsSuccess = false, Errors = errors.ToList() };
    }
}
