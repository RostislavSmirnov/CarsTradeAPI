using Moq;
using Microsoft.Extensions.Logging;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using CarsTradeAPI.Features.CarModelOperation;
using CarsTradeAPI.Features.CarModelOperation.CreateCarModel;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Entities.Elements;
using CarsTradeAPI.Features.CarModelOperation.GetCarModelById;
using CarsTradeAPI.Features.CarModelOperation.EditCarModel;
using CarsTradeAPI.Features.CarModelOperation.DeleteCarModel;


namespace CarsTradeTests.UnitTests.CarModelTests
{
    /// <summary>
    /// Класс для тестирования контроллера CarModelController
    /// </summary>
    public class CarModelControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<CarModelController>> _loggerMock;
        private readonly CarModelController _controller;
        public CarModelControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<CarModelController>>();
            _controller = new CarModelController(_mediatorMock.Object, _loggerMock.Object);
        }


        [Fact]
        public async Task CreateCarModel_ShouldReturnOk_WhenBuyerCreatedSuccessfully()
        {
            // Arrange
            var command = new CreateCarModelCommand
            {
                IdempotencyKey = "abc123",
                CarModelName = "Q8",
                CarColor = "Red",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    CarMusicBrand = "Bose",
                    WheelSize = 19,
                },
                CarCountry = "Russia",
                CarEngine = new Engine
                {
                    EngineCapacity = 3,
                    EngineHorsePower = 210,
                    FuelType = "Gasoline",
                },
                CarManufacturer = "Audi",
                ProductionDateTime = DateTime.UtcNow,
                CarPrice = 3500000
            };

            var expectedCarModel = new ShowCarModelDto
            {
                CarModelName = "Q8",
                CarColor = "Red",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    CarMusicBrand = "Bose",
                    WheelSize = 19,
                },
                CarCountry = "Russia",
                CarEngine = new Engine
                {
                    EngineCapacity = 3,
                    EngineHorsePower = 210,
                    FuelType = "Gasoline",
                },
                CarManufacturer = "Audi",
                ProductionDateTime = DateTime.UtcNow,
                CarPrice = 3500000
            };

            var result = MbResult<ShowCarModelDto>.Success(expectedCarModel);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateCarModelCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var okresult = await _controller.CreateCarModel(command);

            // Assert
            var okResult = okresult.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            var returnedCarModel = okResult.Value as ShowCarModelDto;
            returnedCarModel.Should().NotBeNull();
            returnedCarModel.CarModelId.Should().Be(expectedCarModel.CarModelId);
            returnedCarModel.CarManufacturer.Should().Be(expectedCarModel.CarManufacturer);
            returnedCarModel.CarModelName.Should().Be(expectedCarModel.CarModelName);
            returnedCarModel.CarColor.Should().Be(expectedCarModel.CarColor);
            _mediatorMock.Verify(m => m.Send(It.IsAny<CreateCarModelCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task CreateCarModel_ShouldReturnBadRequest_WhenCarModelCreationFails()
        {
            // Arrange
            var command = new CreateCarModelCommand
            {
                IdempotencyKey = "abc123",
                CarModelName = "Q8",
                CarColor = "Red",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "",
                    CarMusicBrand = "Bose",
                    WheelSize = 19,
                },
                CarCountry = "Russia",
                CarEngine = new Engine
                {
                    EngineCapacity = 3,
                    EngineHorsePower = 210,
                    FuelType = "Gasoline",
                },
                CarManufacturer = "",
                ProductionDateTime = DateTime.UtcNow,
                CarPrice = 35000000
            };
            var failure = MbResult<ShowCarModelDto>.Failure(new[]
            {
                new ErrorDetail("VALIDATION_ERROR", "Ошибка валидации при создании новой модели автомобиля", "CreateCarModel"),
            });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateCarModelCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failure);

            // Act
            var result = await _controller.CreateCarModel(command);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
        }


        [Fact]
        public async Task GetCarModelById_ShouldReturnOk_WhenCarModelExists()
        {
            // Arrange
            var carModelId = Guid.NewGuid();
            var expectedCarModel = new ShowCarModelDto
            {
                CarModelId = carModelId,
                CarModelName = "Q8",
                CarColor = "Red",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    CarMusicBrand = "Bose",
                    WheelSize = 19,
                },
                CarCountry = "Russia",
                CarEngine = new Engine
                {
                    EngineCapacity = 3,
                    EngineHorsePower = 210,
                    FuelType = "Gasoline",
                },
                CarManufacturer = "Audi",
                ProductionDateTime = DateTime.UtcNow,
                CarPrice = 3500000
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetCarModelByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MbResult<ShowCarModelDto>.Success(expectedCarModel));

            // Act
            var result = await _controller.GetCarModelById(carModelId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            var returnedCarModel = okResult.Value as ShowCarModelDto;
            returnedCarModel.Should().NotBeNull();
            returnedCarModel.CarModelId.Should().Be(expectedCarModel.CarModelId);
            returnedCarModel.CarManufacturer.Should().Be(expectedCarModel.CarManufacturer);
            returnedCarModel.CarModelName.Should().Be(expectedCarModel.CarModelName);
            returnedCarModel.CarColor.Should().Be(expectedCarModel.CarColor);
            _mediatorMock.Verify(m => m.Send(It.IsAny<GetCarModelByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }



        [Fact]
        public async Task GetCarModelById_ShouldReturnNotFound_WhenCarModelDoesNotExist()
        {
            // Arrange
            var carModelId = Guid.NewGuid();
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetCarModelByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MbResult<ShowCarModelDto>.Failure(new[]
                {
                new ErrorDetail("NOT_FOUND", "Модель автомобиля не найдена", "GetCarModelById")
                }));
            // Act
            var result = await _controller.GetCarModelById(carModelId);
            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }


        [Fact]
        public async Task EditCarModelById_ShouldReturnOk_WhenCarModelEditedSuccessfully()
        {
            // Arrange
            var command = new EditCarModelCommand
            {
                IdempotencyKey = "abc123",
                CarModelId = Guid.NewGuid(),
                CarModelName = "Q8",
                CarColor = "Red",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    CarMusicBrand = "Bose",
                    WheelSize = 19,
                },
                CarCountry = "Russia",
                CarEngine = new Engine
                {
                    EngineCapacity = 3,
                    EngineHorsePower = 210,
                    FuelType = "Gasoline",
                },
                CarManufacturer = "Audi",
                ProductionDateTime = DateTime.UtcNow,
                CarPrice = 3500000,
            };

            var expectedCarModel = new ShowCarModelDto
            {
                CarModelId = (Guid)command.CarModelId,
                CarModelName = "Q9",
                CarColor = "Black",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    CarMusicBrand = "BW",
                    WheelSize = 19,
                },
                CarCountry = "Germany",
                CarEngine = new Engine
                {
                    EngineCapacity = 4,
                    EngineHorsePower = 320,
                    FuelType = "Gasoline",
                },
                CarManufacturer = "Audi",
                ProductionDateTime = DateTime.UtcNow,
                CarPrice = 7000000
            };

            var result = MbResult<ShowCarModelDto>.Success(expectedCarModel);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<EditCarModelCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var okresult = await _controller.EditCarModel((Guid)command.CarModelId,"idempotencyKey", command);
            
            // Assert
            var okResult = okresult.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            var returnedCarModel = okResult.Value as ShowCarModelDto;
            returnedCarModel.Should().NotBeNull();
            returnedCarModel.CarModelId.Should().Be(expectedCarModel.CarModelId);
            returnedCarModel.CarManufacturer.Should().Be(expectedCarModel.CarManufacturer);
            returnedCarModel.CarModelName.Should().Be(expectedCarModel.CarModelName);
            returnedCarModel.CarColor.Should().Be(expectedCarModel.CarColor);
        }


        [Fact]
        public async Task EditCarModel_ShouldReturnBadRequest_WhenCarModelEditFails()
        {
            // Arrange
            var command = new EditCarModelCommand
            {
                IdempotencyKey = "abc123",
                CarModelId = Guid.NewGuid(),
                CarModelName = "Q8",
                CarColor = "",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "",
                    CarMusicBrand = "Bose",
                    WheelSize = 19,
                },
                CarCountry = "Russia",
                CarEngine = new Engine
                {
                    EngineCapacity = 3,
                    EngineHorsePower = 210,
                    FuelType = "Gasoline",
                },
                CarManufacturer = "",
                ProductionDateTime = DateTime.UtcNow.AddDays(10),
                CarPrice = -35000000
            };
            var failure = MbResult<ShowCarModelDto>.Failure(new[]
            {
                new ErrorDetail("VALIDATION_ERROR", "Ошибка валидации при редактировании модели автомобиля", "EditCarModel"),
            });
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<EditCarModelCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failure);
            // Act
            var result = await _controller.EditCarModel((Guid)command.CarModelId,"idempotencyKey", command);
            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
        }


        [Fact]
        public async Task DeleteCarModel_ShouldReturnOk_WhenCarModelDeletedSuccessfully() 
        {
            // Arrange
            var carModelId = Guid.NewGuid();

            var expectedCarModel = new ShowCarModelDto
            {
                CarModelId = carModelId,
                CarModelName = "Q8",
                CarColor = "Red",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    CarMusicBrand = "Bose",
                    WheelSize = 19,
                },
                CarCountry = "Russia",
                CarEngine = new Engine
                {
                    EngineCapacity = 3,
                    EngineHorsePower = 210,
                    FuelType = "Gasoline",
                },
                CarManufacturer = "Audi",
                ProductionDateTime = DateTime.UtcNow,
                CarPrice = 3500000
            };

            var success = MbResult<ShowCarModelDto>.Success(expectedCarModel);
            
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<DeleteCarModelCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(success);

            // Act
            var result = await _controller.DeleteCarModel(carModelId, "idempotencyKey");

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            var returnedCarModel = okResult.Value as ShowCarModelDto;
            returnedCarModel.Should().NotBeNull();
            returnedCarModel.CarModelId.Should().Be(expectedCarModel.CarModelId);
            returnedCarModel.CarManufacturer.Should().Be(expectedCarModel.CarManufacturer);
            returnedCarModel.CarModelName.Should().Be(expectedCarModel.CarModelName);
            returnedCarModel.CarColor.Should().Be(expectedCarModel.CarColor);
            _mediatorMock.Verify(m => m.Send(It.IsAny<DeleteCarModelCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
