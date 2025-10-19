using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Features.CarModelOperation.CreateCarModel;
using CarsTradeAPI.Features.CarModelOperation.DeleteCarModel;
using CarsTradeAPI.Features.CarModelOperation.EditCarModel;
using CarsTradeAPI.Features.CarModelOperation.GetCarModel;
using CarsTradeAPI.Features.CarModelOperation.GetCarModelById;
using CarsTradeAPI.Infrastructure.Services.ControllerBaseService;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Features.CarModelOperation.SearchCarModel;
using Microsoft.AspNetCore.Authorization;


namespace CarsTradeAPI.Features.CarModelOperation
{
    /// <summary>
    /// Контроллер для управления операциями с моделями автомобилей.
    /// Предоставляет API-методы для создания, редактирования, удаления, получения и поиска моделей автомобилей.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CarModelController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CarModelController> _logger;
        public CarModelController(IMediator mediator, ILogger<CarModelController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }


        /// <summary>
        /// Создает новую модель автомобиля на основе переданных данных.
        /// </summary>
        /// <param name="command">Команда, содержащая данные для создания модели автомобиля.</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowCarModelDto"/> с информацией о созданной модели автомобиля
        /// или ошибку, если создание не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает POST-запрос с телом, содержащим данные о новой модели автомобиля.
        /// </remarks>
        [HttpPost("CreateCarModel")]
        public async Task<ActionResult<ShowCarModelDto>> CreateCarModel([FromBody] CreateCarModelCommand command)
        {
            _logger.LogInformation("Начало обработки запроса CreateCarModel");
            MbResult<ShowCarModelDto> newCarModel = await _mediator.Send(command);
            return HandleResult(newCarModel);
        }


        /// <summary>
        /// Удаляет модель автомобиля по ее уникальному идентификатору.
        /// </summary>
        /// <param name="carModelId">Уникальный идентификатор модели автомобиля (GUID).</param>
        /// <param name="idempotencyKey">Ключ идемпотентности.</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowCarModelDto"/> с информацией об удаленной модели
        /// или ошибку, если удаление не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает DELETE-запрос с указанием идентификатора модели автомобиля в URL.
        /// </remarks>
        [HttpDelete("DeleteCarModel/{carModelId:Guid}")]
        public async Task<ActionResult<ShowCarModelDto>> DeleteCarModel(Guid carModelId, [FromHeader(Name = "idempotencyKey")] string idempotencyKey)
        {
            _logger.LogInformation("Начало обработки запроса DeleteCarModel для CarModelId: {CarModelId}", carModelId);
            DeleteCarModelCommand command = new DeleteCarModelCommand { CarModelId = carModelId, IdempotencyKey = idempotencyKey };
            MbResult<ShowCarModelDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }


        /// <summary>
        /// Обновляет информацию о модели автомобиля по ее уникальному идентификатору.
        /// </summary>
        /// <param name="CarModelId">Уникальный идентификатор модели автомобиля (GUID), передаваемый в URL.</param>
        /// <param name="command">Команда, содержащая обновленные данные о модели автомобиля.</param>
        /// <param name="idempotencyKey">Ключ идемпотентности.</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowCarModelDto"/> с обновленной информацией о модели
        /// или ошибку, если редактирование не удалось.
        /// </returns>
        /// <remarks>
        /// Метод принимает PUT-запрос с идентификатором модели автомобиля в URL и телом, содержащим обновленные данные.
        /// </remarks>
        [HttpPut("EditCarModel/{CarModelId:Guid}")]
        public async Task<ActionResult<ShowCarModelDto>> EditCarModel(Guid CarModelId, [FromHeader(Name = "idempotencyKey")] string idempotencyKey, [FromBody] EditCarModelCommand command)
        {
            _logger.LogInformation("Начало обработки запроса EditCarModel для CarModelId: {CarModelId}", CarModelId);
            command.CarModelId = CarModelId;
            command.IdempotencyKey = idempotencyKey;
            MbResult<ShowCarModelDto> updatedCarModelDto = await _mediator.Send(command);
            return HandleResult(updatedCarModelDto);
        }


        /// <summary>
        /// Получает список всех моделей автомобилей.
        /// </summary>
        /// <returns>
        /// Возвращает список объектов <see cref="ShowCarModelDto"/> с информацией о всех моделях автомобилей
        /// или ошибку, если запрос не выполнен.
        /// </returns>
        /// <remarks>
        /// Метод принимает GET-запрос и возвращает полный список моделей автомобилей.
        /// </remarks>
        [HttpGet("GetCarModels/All")]
        public async Task<ActionResult<List<ShowCarModelDto>>> GetCarModel()
        {
            _logger.LogInformation("Начало обработки запроса GetCarModel для получения всех моделей");
            GetCarModelQuery query = new GetCarModelQuery();
            MbResult<List<ShowCarModelDto>> result = await _mediator.Send(query);
            return HandleResult(result);
        }


        /// <summary>
        /// Получает информацию о модели автомобиля по ее уникальному идентификатору.
        /// </summary>
        /// <param name="carModelId">Уникальный идентификатор модели автомобиля (GUID).</param>
        /// <returns>
        /// Возвращает объект <see cref="ShowCarModelDto"/> с информацией о модели автомобиля
        /// или ошибку, если модель не найдена.
        /// </returns>
        /// <remarks>
        /// Метод принимает GET-запрос с указанием идентификатора модели автомобиля в URL.
        /// </remarks>
        [HttpGet("GetCarModel/{carModelId:Guid}")]
        public async Task<ActionResult<ShowCarModelDto>> GetCarModelById(Guid carModelId)
        {
            _logger.LogInformation("Начало обработки запроса GetCarModelById для CarModelId: {CarModelId}", carModelId);
            GetCarModelByIdQuery command = new GetCarModelByIdQuery { CarModelId = carModelId };
            MbResult<ShowCarModelDto> result = await _mediator.Send(command);
            return HandleResult(result);
        }


        /// <summary>
        /// Выполняет поиск моделей автомобилей по заданным параметрам.
        /// </summary>
        /// <param name="query">Запрос, содержащий параметры для поиска моделей автомобилей.</param>
        /// <returns>
        /// Возвращает список объектов <see cref="ShowCarModelDto"/> с информацией о моделях, соответствующих критериям поиска,
        /// или ошибку, если запрос не выполнен.
        /// </returns>
        /// <remarks>
        /// Метод принимает POST-запрос с телом, содержащим параметры поиска.
        /// </remarks>
        [HttpPost("GetCarModels/Search")]
        public async Task<ActionResult<List<ShowCarModelDto>>> SearchCarModels([FromBody] SearchCarModelQuery query)
        {
            _logger.LogInformation("Начало обработки запроса на поиск автомобилей по параметрам");
            MbResult<List<ShowCarModelDto>> result = await _mediator.Send(query);
            return HandleResult(result);
        }
    }
}
