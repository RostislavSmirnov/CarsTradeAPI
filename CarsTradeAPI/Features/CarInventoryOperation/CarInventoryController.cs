using Microsoft.AspNetCore.Mvc;
using MediatR;
using CarsTradeAPI.Features.CarInventoryOperation.CreateCarInventory;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;
using CarsTradeAPI.Features.CarInventoryOperation.DeleteCarInventory;
using CarsTradeAPI.Features.CarInventoryOperation.GetCarInventory;
using CarsTradeAPI.Features.CarInventoryOperation.GetCarInventoryById;
using CarsTradeAPI.Features.CarInventoryOperation.EditCarInventory;
using CarsTradeAPI.Infrastructure.Services.ControllerBaseService;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using Microsoft.AspNetCore.Authorization;


namespace CarsTradeAPI.Features.CarInventoryOperation
{
    /// <summary>
    /// Контроллер для управления операциями с инвентарем автомобилей.
    /// Предоставляет API-методы для создания, редактирования, удаления и получения информации об объектах автомобильного инвентаря.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CarInventoryController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CarInventoryController> _logger;
        public CarInventoryController(IMediator mediator, ILogger<CarInventoryController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }


        /// <summary>
        /// Создает новый объект автомобильного инвентаря на основе переданных данных.
        /// </summary>
        /// <param name="command">Команда, содержащая данные для создания объекта инвентаря.</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowCarInventoryDto"/> с информацией о созданном объекте инвентаря
        /// или ошибку, если создание не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает POST-запрос с телом, содержащим данные об объекте автомобильного инвентаря.
        /// </remarks>
        [HttpPost("CreateCarInventory")]
        public async Task<ActionResult<ShowCarInventoryDto>> CreateCarInventory([FromBody] CreateCarInventoryCommand command)
        {
            _logger.LogInformation("Получен запрос на создание нового CarInventory");
            MbResult<ShowCarInventoryDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }


        /// <summary>
        /// Удаляет объект автомобильного инвентаря по его уникальному идентификатору.
        /// </summary>
        /// <param name="Id">Уникальный идентификатор объекта инвентаря (GUID).</param>
        /// <param name="idempotencyKey">Уникальный ключ для реализации идемпотентности (передается в хедере X-Idempotency-Key).</param>
        /// <returns>...</returns>
        [HttpDelete("DeleteCarInventory/{Id:Guid}")]
        public async Task<ActionResult<ShowCarInventoryDto>> DeleteCarInventory(Guid Id, [FromHeader(Name = "idempotencyKey")] string idempotencyKey)
        {
            _logger.LogInformation("Получен запрос на удаление CarInventory с ID: {Id}", Id);
            DeleteCarInventoryCommand command = new DeleteCarInventoryCommand{CarInventoryId = Id,IdempotencyKey = idempotencyKey};
            MbResult<ShowCarInventoryDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }


        /// <summary>
        /// Обновляет информацию об объекте автомобильного инвентаря.
        /// </summary>
        /// <param name="command">Команда, содержащая обновленные данные об объекте инвентаря.</param>
        /// <param name="id">Уникальный идентификатор объекта инвентаря (GUID).</param>
        /// <param name="idempotencyKey">Уникальный ключ для реализации идемпотентности (передается в хедере X-Idempotency-Key).</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowCarInventoryDto"/> с обновленной информацией
        /// или ошибку, если редактирование не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает PUT-запрос с телом, содержащим обновленные данные объекта инвентаря.
        /// </remarks>
        [HttpPut("EditCarInventory/{id:guid}")]
        public async Task<ActionResult<ShowCarInventoryDto>> EditCarInventory(Guid id, [FromHeader(Name ="idempotencyKey")]string idempotencyKey, [FromBody] EditCarInventoryCommand command)
        {
            _logger.LogInformation("Получен запрос на редактирование CarInventory с ID: {Id}", command.InventoryId);
            command.InventoryId = id;
            command.IdempotencyKey = idempotencyKey;
            MbResult<ShowCarInventoryDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }


        /// <summary>
        /// Получает список всех объектов автомобильного инвентаря.
        /// </summary>
        /// <returns>
        /// Возвращает список объектов <see cref="ShowCarInventoryDto"/> с информацией о всех объектах инвентаря
        /// или ошибку, если запрос не выполнен.
        /// </returns>
        /// <remarks>
        /// Метод принимает GET-запрос и возвращает полный список объектов автомобильного инвентаря.
        /// </remarks>
        [HttpGet("GetCarInventory/All")]
        public async Task<ActionResult<List<ShowCarInventoryDto>>> GetAllCarInventories()
        {
            _logger.LogInformation("Получен запрос на получение всех CarInventory");
            MbResult<List<ShowCarInventoryDto>> result = await _mediator.Send(new GetCarInventoryQuery());
            return HandleResult(result);
        }


        /// <summary>
        /// Получает информацию об объекте автомобильного инвентаря по его уникальному идентификатору.
        /// </summary>
        /// <param name="carInventoryId">Уникальный идентификатор объекта инвентаря (GUID).</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowCarInventoryDto"/> с информацией об объекте инвентаря
        /// или ошибку, если объект не найден.
        /// </returns>
        /// <remarks>
        /// Метод принимает GET-запрос с указанием идентификатора объекта инвентаря в URL.
        /// </remarks>
        [HttpGet("GetCarInventory/{carInventoryId:Guid}")]
        public async Task<ActionResult<ShowCarInventoryDto>> GetCarInventoryById(Guid carInventoryId)
        {
            _logger.LogInformation("Получен запрос на получение CarInventory с ID: {carInventoryId}", carInventoryId);
            MbResult<ShowCarInventoryDto> result = await _mediator.Send(new GetCarInventoryByIdQuery { CarInventoryId = carInventoryId });
            return HandleResult(result);
        }
    }
}
