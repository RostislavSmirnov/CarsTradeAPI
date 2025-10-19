using Microsoft.AspNetCore.Mvc;
using MediatR;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Features.EmployeeOperation.CreateEmployee;
using CarsTradeAPI.Features.EmployeeOperation.DeleteEmployee;
using CarsTradeAPI.Features.EmployeeOperation.EditEmployee;
using CarsTradeAPI.Features.EmployeeOperation.GetAllEmployee;
using CarsTradeAPI.Features.EmployeeOperation.GetEmployeeById;
using CarsTradeAPI.Features.EmployeeOperation.CreateAdminForTest;
using CarsTradeAPI.Infrastructure.Services.ControllerBaseService;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using Microsoft.AspNetCore.Authorization;


namespace CarsTradeAPI.Features.EmployeeOperation
{
    /// <summary>
    /// Контроллер для управления операциями с сотрудниками.
    /// Предоставляет API-методы для создания, редактирования, удаления и получения информации о сотрудниках.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EmployeeController> _logger;
        public EmployeeController(IMediator mediator, ILogger<EmployeeController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }


        /// <summary>
        /// Создает нового администратора для тестирования на основе переданных данных.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("CreateAdminForTestCommand")]
        public async Task<ActionResult<ShowEmployeeDto>> CreateAdminForTest([FromBody] CreateAdminForTestCommand command)
        {
            _logger.LogInformation("Начало обработки запроса CreateAdminForTest");
            MbResult<ShowEmployeeDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }


        /// <summary>
        /// Создает нового сотрудника на основе переданных данных.
        /// </summary>
        /// <param name="command">Команда, содержащая данные для создания сотрудника.</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowEmployeeDto"/> с информацией о созданном сотруднике
        /// или ошибку, если создание не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает POST-запрос с телом, содержащим данные о новом сотруднике.
        /// </remarks>
        [HttpPost("CreateEmployee")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ShowEmployeeDto>> CreateEmployee([FromBody] CreateEmployeeCommand command)
        {
            _logger.LogInformation("Начало обработки запроса CreateEmployee");
            MbResult<ShowEmployeeDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }


        /// <summary>
        /// Удаляет сотрудника по его уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор сотрудника (GUID).</param>
        /// <param name="idempotencyKey">Ключ идемпотентности.</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowEmployeeDto"/> с информацией об удаленном сотруднике
        /// или ошибку, если удаление не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает DELETE-запрос с указанием идентификатора сотрудника в URL.
        /// </remarks>
        [HttpDelete("DeleteEmployee/{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ShowEmployeeDto>> DeleteEmployee(Guid id, [FromHeader(Name = "idempotencyKey")] string idempotencyKey)
        {
            _logger.LogInformation("Начало обработки запроса DeleteEmployee для EmployeeId: {EmployeeId}", id);
            MbResult<ShowEmployeeDto> result = await _mediator.Send(new DeleteEmployeeCommand { EmployeeId = id, IdempotencyKey = idempotencyKey });
            return HandleResult(result);
        }


        /// <summary>
        /// Обновляет информацию о сотруднике по его уникальному идентификатору.
        /// </summary>
        /// <param name="id">Уникальный идентификатор сотрудника (GUID), передаваемый в URL.</param>
        /// <param name="command">Команда, содержащая обновленные данные о сотруднике.</param>
        /// <param name="idempotencyKey"> Ключ идемпотентности.</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowEmployeeDto"/> с обновленной информацией о сотруднике
        /// или ошибку, если редактирование не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает PUT-запрос с идентификатором сотрудника в URL и телом, содержащим обновленные данные.
        /// </remarks>
        [HttpPut("EditEmployee/{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ShowEmployeeDto>> EditEmployee(Guid id, [FromHeader(Name = "idempotencyKey")] string idempotencyKey, [FromBody] EditEmployeeCommand command)
        {
            _logger.LogInformation("Начало обработки запроса EditEmployee для EmployeeId: {EmployeeId}", id);
            command.EmployeeId = id;
            command.IdempotencyKey = idempotencyKey;
            MbResult<ShowEmployeeDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }


        /// <summary>
        /// Получает список всех сотрудников.
        /// </summary>
        /// <returns>
        /// Возвращает список объектов <see cref="ShowEmployeeDto"/> с информацией о всех сотрудниках
        /// или ошибку, если запрос не выполнен.
        /// </returns>
        /// <remarks>
        /// Метод принимает GET-запрос и возвращает полный список сотрудников.
        /// </remarks>
        [HttpGet("GetAllEmployee/All")]
        [Authorize]
        public async Task<ActionResult<List<ShowEmployeeDto>>> GetAllEmployee()
        {
            _logger.LogInformation("Начало обработки запроса GetAllEmployee");
            MbResult<List<ShowEmployeeDto>> result = await _mediator.Send(new GetAllEmployeeQuery());
            return HandleResult(result);
        }


        /// <summary>
        /// Получает информацию о сотруднике по его уникальному идентификатору.
        /// </summary>
        /// <param name="employeeId">Уникальный идентификатор сотрудника (GUID).</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowEmployeeDto"/> с информацией о сотруднике
        /// или ошибку, если сотрудник не найден.
        /// </returns>
        /// <remarks>
        /// Метод принимает GET-запрос с указанием идентификатора сотрудника в URL.
        /// </remarks>
        [HttpGet("GetEmployeeById/{employeeId:Guid}")]
        [Authorize]
        public async Task<ActionResult<ShowEmployeeDto>> GetEmployeeById(Guid employeeId)
        {
            _logger.LogInformation("Начало обработки запроса GetEmployeeById для EmployeeId: {EmployeeId}", employeeId);
            MbResult<ShowEmployeeDto> result = await _mediator.Send(new GetEmployeeByIdQuery { EmployeeId = employeeId });
            return HandleResult(result);
        }
    }
}
