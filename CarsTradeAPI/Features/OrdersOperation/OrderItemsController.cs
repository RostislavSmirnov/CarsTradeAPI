using Microsoft.AspNetCore.Mvc;
using MediatR;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Features.OrdersOperation.OrderItems.DeleteOrderItems;
using CarsTradeAPI.Features.OrdersOperation.OrderItems.EditOrderItem;
using CarsTradeAPI.Features.OrdersOperation.OrderItems.AddOrderItem;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Infrastructure.Services.ControllerBaseService;
using CarsTradeAPI.Features.OrdersOperation.OrderItems.AddOrderItems;

namespace CarsTradeAPI.Features.OrdersOperation
{
    /// <summary>
    /// Контроллер для управления операциями с элементами заказов.
    /// Предоставляет API-методы для добавления, редактирования и удаления элементов заказов.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemsController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderItemsController> _logger;
        public OrderItemsController(IMediator mediator, ILogger<OrderItemsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }


        /// <summary>
        /// Добавляет новый элемент в заказ на основе переданных данных.
        /// </summary>
        /// <param name="command">Команда, содержащая данные для добавления элемента заказа.</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowOrderDto"/> с информацией об обновленном заказе
        /// или ошибку, если добавление не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает POST-запрос с телом, содержащим данные о новом элементе заказа, включая идентификатор заказа.
        /// </remarks>
        [HttpPost("AddOrderItem")]
        public async Task<ActionResult<ShowOrderDto>> AddOrderItems([FromBody] CreateOrderItemCommand command)
        {
            _logger.LogInformation("Получение запроса на добавление нового элемента заказа в заказ с ID: {OrderId}", command.OrderId);
            MbResult<ShowOrderDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }


        /// <summary>
        /// Добавляет несколько новых элементов в заказ на основе переданных данных.
        /// </summary>
        /// <param name="command">Команда, содержащая данные для добавления нескольких элементов заказа.</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowOrderDto"/> с информацией об обновленном заказе
        /// или ошибку, если добавление не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает POST-запрос с телом, содержащим данные о новых элементах заказа, включая идентификатор заказа.
        /// </remarks>
        [HttpPost("AddOrderItems")]
        public async Task<ActionResult<ShowOrderDto>> AddOrderItems([FromBody] AddOrderItemsCommand command)
        {
            _logger.LogInformation("Получение запроса на добавление новых элементов заказа в заказ с ID: {OrderId}", command.OrderId);
            MbResult<ShowOrderDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }


        /// <summary>
        /// Удаляет элемент заказа по его идентификатору и идентификатору заказа.
        /// </summary>
        /// <param name="command">Команда, содержащая идентификатор заказа и идентификатор элемента заказа для удаления.</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowOrderDto"/> с информацией об обновленном заказе
        /// или ошибку, если удаление не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает DELETE-запрос с телом, содержащим идентификатор заказа и идентификатор элемента заказа.
        /// </remarks>
        [HttpDelete("DeleteOrderItems")]
        public async Task<ActionResult<ShowOrderDto>> DeleteOrderItems([FromBody] DeleteOrderItemsCommand command)
        {
            _logger.LogInformation("Получение запроса на удаление элемента заказа с ID: {OrderItemId} из заказа с ID: {OrderId}", command.OrderItemId, command.OrderId);
            MbResult<ShowOrderDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }


        /// <summary>
        /// Обновляет информацию об элементе заказа.
        /// </summary>
        /// <param name="command">Команда, содержащая обновленные данные об элементе заказа, включая идентификаторы заказа и элемента.</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowOrderDto"/> с информацией об обновленном заказе
        /// или ошибку, если редактирование не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает PUT-запрос с телом, содержащим обновленные данные об элементе заказа.
        /// </remarks>
        [HttpPut("EditOrderItem")]
        public async Task<ActionResult<ShowOrderDto>> EditOrderItem([FromBody] EditOrderItemCommand command)
        {
            _logger.LogInformation("Получение запроса на редактирование элемента заказа с ID: {OrderItemId} в заказе с ID {OrderId}", command.OrderItemId, command.OrderId);
            MbResult<ShowOrderDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }
    }
}