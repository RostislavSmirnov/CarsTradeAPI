using CarsTradeAPI.Infrastructure.ValidationBehavior;
using Microsoft.AspNetCore.Mvc;


namespace CarsTradeAPI.Infrastructure.Services.ControllerBaseService
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Обрабатывает результат с данными (MbResult&lt;T&gt;)
        /// </summary>
        protected ActionResult HandleResult<T>(MbResult<T> result)
        {
            if (result.IsSuccess)
            {
                // Если успешный результат, возвращаем данные
                return Ok(result.Value);
            }

            // Если есть ошибки
            // можно настроить различные коды в зависимости от Code
            if (result.Errors.Any(e => e.Code == "NOT_FOUND"))
                return NotFound(result.Errors);

            if (result.Errors.Any(e => e.Code == "VALIDATION_ERROR"))
                return BadRequest(result.Errors);

            // По умолчанию — 400
            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Обрабатывает результат без данных (MbResult без T)
        /// </summary>
        protected ActionResult HandleResult(MbResult result)
        {
            if (result.IsSuccess)
            {
                // Для операций без возвращаемых данных (например DELETE)
                return NoContent();
            }

            if (result.Errors.Any(e => e.Code == "NOT_FOUND"))
                return NotFound(result.Errors);

            if (result.Errors.Any(e => e.Code == "VALIDATION_ERROR"))
                return BadRequest(result.Errors);

            return BadRequest(result.Errors);
        }
    }
}
