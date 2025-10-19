using CarsTradeAPI.Infrastructure.Services.ControllerBaseService;
using MediatR;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeAuthentication;
using Microsoft.AspNetCore.Mvc;
using CarsTradeAPI.Infrastructure.ValidationBehavior;


namespace CarsTradeAPI.Features.EmployeeOperation
{
    /// <summary>
    /// Контроллер для аутентификации сотрудников.
    /// </summary>
    [Route("api/[controller]")]
    public class EmployeeAuthenticationController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EmployeeAuthenticationController> _logger;
        public EmployeeAuthenticationController(IMediator mediator, ILogger<EmployeeAuthenticationController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }


        /// <summary>
        /// Аутентифицирует сотрудника на основе переданных учетных данных.
        /// </summary>
        /// <param name="command">Команда, содержащая логин и пароль сотрудника.</param>
        /// <returns>
        /// Возвращает объект <see cref="AuthResultDto"/> с результатами аутентификации
        /// или ошибку, если аутентификация не удалась.
        /// </returns>
        /// <remarks>
        /// Метод принимает POST-запрос с телом, содержащим логин и пароль сотрудника.
        /// </remarks>
        [HttpPost("Authenticate")]
        public async Task<ActionResult<AuthResultDto>> Authenticate([FromBody] EmployeeAuthenticationCommand command)
        {
            _logger.LogInformation("Начало обработки запроса аутентификации сотрудника");
            MbResult<AuthResultDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }
    }
}
