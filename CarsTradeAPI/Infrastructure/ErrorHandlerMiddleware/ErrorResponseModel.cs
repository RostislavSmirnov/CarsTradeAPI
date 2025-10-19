namespace CarsTradeAPI.Infrastructure.ErrorHandlerMiddleware
{
    /// <summary>
    /// Модель для представления ошибки в ответе API
    /// </summary>
    public class ErrorResponseModel
    {
        public int StatusCode { get; set; }

        public DateTime Timestamp { get; set; }

        public string Message { get; set; } = null!;

        public string? Details { get; set; }
    }
}
