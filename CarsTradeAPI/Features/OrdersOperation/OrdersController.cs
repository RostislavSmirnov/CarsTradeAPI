using Microsoft.AspNetCore.Mvc;
using MediatR;
using CarsTradeAPI.Features.OrdersOperation.CreateOrder;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Features.OrdersOperation.DeleteOrder;
using CarsTradeAPI.Features.OrdersOperation.EditOrder;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.ControllerBaseService;
using CarsTradeAPI.Features.OrdersOperation.GetOrderById;
using CarsTradeAPI.Features.OrdersOperation.GetAllOrders;
using Microsoft.AspNetCore.Authorization;


namespace CarsTradeAPI.Features.OrdersOperation
{
    /// <summary>
    /// Контроллер для управления операциями с заказами.
    /// Предоставляет API-методы для создания, редактирования, удаления и получения информации о заказах.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrdersController> _logger;
        public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }


        /// <summary>
        /// Создает новый заказ на основе переданных данных.
        /// </summary>
        /// <param name="command">Команда, содержащая данные для создания заказа.</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowOrderDto"/> с информацией о созданном заказе
        /// или ошибку, если создание не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает POST-запрос с телом, содержащим данные о новом заказе.
        /// </remarks>
        [HttpPost("CreateOrder")]
        public async Task<ActionResult<ShowOrderDto>> CreateOrder([FromBody] CreateOrderCommand command)
        {
            _logger.LogInformation("Получен запрос на создание нового заказа");
            MbResult<ShowOrderDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }


        /// <summary>
        /// Удаляет заказ по его уникальному идентификатору.
        /// </summary>
        /// <param name="Id">Уникальный идентификатор заказа (GUID).</param>
        /// <param name="idempotencyKey"> Ключ идемпотентности (GUID).</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowOrderDto"/> с информацией об удаленном заказе
        /// или ошибку, если удаление не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает DELETE-запрос с указанием идентификатора заказа в URL.
        /// </remarks>
        [HttpDelete("DeleteOrder/{Id:Guid}")]
        public async Task<ActionResult<ShowOrderDto>> DeleteOrder(Guid Id,[FromHeader(Name = "idempotencyKey")] string idempotencyKey)
        {
            _logger.LogInformation("Получение запроса на удаление заказа с ID: {OrderId}", Id);
            DeleteOrderCommand command = new DeleteOrderCommand{ OrderId = Id, IdempotencyKey = idempotencyKey};
            MbResult<ShowOrderDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }


        /// <summary>
        /// Обновляет информацию о заказе по его уникальному идентификатору.
        /// </summary>
        /// <param name="Id">Уникальный идентификатор заказа (GUID), передаваемый в URL.</param>
        /// <param name="command">Команда, содержащая данные для редакирования заказа.</param>
        /// <param name="idempotencyKey"> Ключ идемпотентности (GUID).</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowOrderDto"/> с обновленной информацией о заказе
        /// или ошибку, если редактирование не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает PUT-запрос с указанием идентификатора заказа в URL.
        /// </remarks>
        [HttpPut("EditOrder/{Id:Guid}")]
        public async Task<ActionResult<ShowOrderDto>> EditOrder(Guid Id, [FromHeader(Name = "idempotencyKey")] string idempotencyKey, [FromBody] EditOrderCommand command)
        {
            _logger.LogInformation("Получение запроса на редактирование информации о заказе с ID: {Id}", Id);
            command.OrderId = Id;
            command.IdempotencyKey = idempotencyKey;
            MbResult<ShowOrderDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }


        /// <summary>
        /// Получает информацию о заказе по его уникальному идентификатору.
        /// </summary>
        /// <param name="orderId">Уникальный идентификатор заказа (GUID).</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowOrderDto"/> с информацией о заказе
        /// или ошибку, если заказ не найден.
        /// </returns>
        /// <remarks>
        /// Метод принимает GET-запрос с указанием идентификатора заказа в URL.
        /// </remarks>
        [HttpGet("GetOrderById/{orderId:Guid}")]
        public async Task<ActionResult<ShowOrderDto>> GetOrderById(Guid orderId)
        {
            _logger.LogInformation("Получение запроса на выдачу заказа по ID: {orderId}", orderId);
            MbResult<ShowOrderDto> result = await _mediator.Send(new GetOrderByIdQuery { Id = orderId });
            return HandleResult(result);
        }


        /// <summary>
        /// Получает список всех заказов.
        /// </summary>
        /// <returns>
        /// Возвращает список объектов <see cref="ShowOrderDto"/> с информацией о всех заказах
        /// или ошибку, если запрос не выполнен.
        /// </returns>
        /// <remarks>
        /// Метод принимает GET-запрос и возвращает полный список заказов.
        /// </remarks>
        [HttpGet("GetAllOrders/All")]
        public async Task<ActionResult<List<ShowOrderDto>>> GetAllOrders()
        {
            _logger.LogInformation("Получение запроса на выдачу всех заказов");
            MbResult<List<ShowOrderDto>> result = await _mediator.Send(new GetAllOrdersQuery());
            return HandleResult(result);
        }
    }
}