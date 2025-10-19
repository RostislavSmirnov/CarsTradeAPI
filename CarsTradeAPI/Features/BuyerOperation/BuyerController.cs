using Microsoft.AspNetCore.Mvc;
using MediatR;
using CarsTradeAPI.Features.BuyerOperation.DeleteBuyer;
using CarsTradeAPI.Features.BuyerOperation.CreateBuyer;
using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using CarsTradeAPI.Features.BuyerOperation.EditBuyer;
using CarsTradeAPI.Features.BuyerOperation.GetBuyers;
using CarsTradeAPI.Features.BuyerOperation.GetBuyerById;
using CarsTradeAPI.Infrastructure.Services.ControllerBaseService;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using Microsoft.AspNetCore.Authorization;


namespace CarsTradeAPI.Features.BuyerOperation
{
    /// <summary>
    /// Контроллер для управления операциями с покупателями.
    /// Предоставляет API-методы для создания, редактирования, удаления и получения информации о покупателях.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BuyerController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BuyerController> _logger;
        public BuyerController(IMediator mediator, ILogger<BuyerController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }


        /// <summary>
        /// Создает нового покупателя на основе переданных данных.
        /// </summary>
        /// <param name="createBuyerCommand">Команда, содержащая данные для создания покупателя.</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowBuyerDto"/> с информацией о созданном покупателе
        /// или ошибку, если создание не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает POST-запрос с телом, содержащим данные о новом покупателе.
        /// </remarks>
        [HttpPost("CreateBuyer")]
        public async Task<ActionResult<ShowBuyerDto>> CreateBuyer([FromBody] CreateBuyerCommand createBuyerCommand)
        {
            _logger.LogInformation("Получен запрос на создание нового покупателя");
            MbResult<ShowBuyerDto> result = await _mediator.Send(createBuyerCommand);
            return HandleResult(result);
        }


        /// <summary>
        /// Удаляет покупателя по его уникальному идентификатору.
        /// </summary>
        /// <param name="Id">Уникальный идентификатор покупателя (GUID).</param>
        /// <param name="idempotencyKey">Уникальный ключ идемпотентности(GUID).</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowBuyerDto"/> с информацией об удаленном покупателе
        /// или ошибку, если удаление не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает DELETE-запрос с указанием идентификатора покупателя в URL.
        /// </remarks>
        [HttpDelete("DeleteBuyer/{Id:Guid}")]
        public async Task<ActionResult<ShowBuyerDto>> DeleteBuyer(Guid Id, [FromHeader(Name = "X-Idempotency-Key")] string idempotencyKey)
        {
            _logger.LogInformation("Получен запрос на удаление покупателя с ID: {BuyerId}", Id);
            DeleteBuyerCommand deleteBuyerCommand = new DeleteBuyerCommand { BuyerId = Id, IdempotencyKey = idempotencyKey };
            MbResult<ShowBuyerDto> deletedBuyer = await _mediator.Send(deleteBuyerCommand);
            return HandleResult(deletedBuyer);
        }


        /// <summary>
        /// Обновляет информацию о существующем покупателе.
        /// </summary>
        /// <param name="editBuyerCommand">Команда, содержащая обновленные данные о покупателе.</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowBuyerDto"/> с обновленной информацией о покупателе
        /// или ошибку, если редактирование не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает PUT-запрос с телом, содержащим обновленные данные покупателя.
        /// </remarks>
        [HttpPut("EditBuyer")]
        public async Task<ActionResult<ShowBuyerDto>> EditBuyer(EditBuyerCommand editBuyerCommand)
        {
            _logger.LogInformation("Получен запрос на редактирование покупателя с ID: {BuyerId}", editBuyerCommand.BuyerId);
            MbResult<ShowBuyerDto> updatedBuyer = await _mediator.Send(editBuyerCommand);
            return HandleResult(updatedBuyer);
        }


        /// <summary>
        /// Получает список всех покупателей.
        /// </summary>
        /// <returns>
        /// Возвращает список объектов <see cref="ShowBuyerDto"/> с информацией о всех покупателях
        /// или ошибку, если запрос не выполнен.
        /// </returns>
        /// <remarks>
        /// Метод принимает GET-запрос и возвращает полный список покупателей.
        /// </remarks>
        [HttpGet("GetAllBuyers")]
        public async Task<ActionResult<List<ShowBuyerDto>>> GetAllBuyers()
        {
            _logger.LogInformation("Получен запрос на получение всех покупателей");
            MbResult<List<ShowBuyerDto>> buyers = await _mediator.Send(new GetBuyersQuery());
            return HandleResult(buyers);
        }


        /// <summary>
        /// Получает информацию о покупателе по его уникальному идентификатору.
        /// </summary>
        /// <param name="buyerId">Уникальный идентификатор покупателя (GUID).</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowBuyerDto"/> с информацией о покупателе
        /// или ошибку, если покупатель не найден.
        /// </returns>
        /// <remarks>
        /// Метод принимает GET-запрос с указанием идентификатора покупателя в URL.
        /// </remarks>
        [HttpGet("GetBuyerById/{buyerId:Guid}")]
        public async Task<ActionResult<ShowBuyerDto>> GetBuyerById(Guid buyerId)
        {
            _logger.LogInformation("Получен запрос на получение покупателя с ID: {BuyerId}", buyerId);
            MbResult<ShowBuyerDto> buyer = await _mediator.Send(new GetBuyerByIdQuery { BuyerId = buyerId });
            return HandleResult(buyer);
        }
    }
}