using Moq;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using CarsTradeAPI.Features.CarInventoryOperation;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;
using CarsTradeAPI.Features.CarInventoryOperation.CreateCarInventory;
using CarsTradeAPI.Features.CarInventoryOperation.DeleteCarInventory;
using CarsTradeAPI.Features.CarInventoryOperation.EditCarInventory;
using CarsTradeAPI.Features.CarInventoryOperation.GetCarInventory;
using CarsTradeAPI.Features.CarInventoryOperation.GetCarInventoryById;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Entities.Elements;


namespace CarsTradeTests.UnitTests.CarInventoryTests
{
    /// <summary>
    /// Тесты для контроллера CarInventoryController
    /// </summary>
    public class CarInventoryControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<CarInventoryController>> _loggerMock;
        private readonly CarInventoryController _controller;

        public CarInventoryControllerTests()
        {
            // Создаем моки для зависимостей
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<CarInventoryController>>();

            // Передаем моки в контроллер
            _controller = new CarInventoryController(_mediatorMock.Object, _loggerMock.Object);
        }


        [Fact]
        public async Task GetAllCarInventories_ReturnsOkResult_WhenDataExists()
        {
            // Arrange
            var fakeList = new List<ShowCarInventoryDto>
            {
                new ShowCarInventoryDto
                {
                    InventoryId = Guid.NewGuid(),
                    CarModelId = Guid.NewGuid(),
                    CarModel = new ShowCarModelDto 
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
                        CarPrice = 3500000,
                        CarModelId = Guid.NewGuid()
                    },
                    Quantity = 5,
                    LastUpdated = DateTime.UtcNow
                }
            };

            var result = MbResult<List<ShowCarInventoryDto>>.Success(fakeList);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetCarInventoryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var response = await _controller.GetAllCarInventories();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var returnedList = Assert.IsAssignableFrom<List<ShowCarInventoryDto>>(okResult.Value);
            Assert.Single(returnedList);
        }


        [Fact]
        public async Task GetCarInventoryById_ReturnsOkResult_WhenFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            var fakeDto = new ShowCarInventoryDto
            {
                InventoryId = id,
                CarModelId = Guid.NewGuid(),
                CarModel = new ShowCarModelDto
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
                    CarPrice = 3500000,
                    CarModelId = Guid.NewGuid()
                },
                Quantity = 3,
                LastUpdated = DateTime.UtcNow
            };

            var result = MbResult<ShowCarInventoryDto>.Success(fakeDto);

            _mediatorMock
                .Setup(x => x.Send(It.Is<GetCarInventoryByIdQuery>(q => q.CarInventoryId == id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var response = await _controller.GetCarInventoryById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var returned = Assert.IsAssignableFrom<ShowCarInventoryDto>(okResult.Value);
            Assert.Equal(id, returned.InventoryId);
        }


        [Fact]
        public async Task CreateCarInventory_ReturnsOkResult_WhenCreated()
        {
            // Arrange
            var command = new CreateCarInventoryCommand
            {
                CarModelId = Guid.NewGuid(),
                Quantity = 10,
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            var fakeDto = new ShowCarInventoryDto
            {
                InventoryId = Guid.NewGuid(),
                CarModelId = command.CarModelId,
                CarModel = new ShowCarModelDto
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
                        FuelType = "Gasoline"                 },
                    CarManufacturer = "Audi",
                    ProductionDateTime = DateTime.UtcNow,
                    CarPrice = 3500000,
                    CarModelId = Guid.NewGuid()
                },
                Quantity = (uint)command.Quantity,
                LastUpdated = DateTime.UtcNow
            };

            var result = MbResult<ShowCarInventoryDto>.Success(fakeDto);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateCarInventoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var response = await _controller.CreateCarInventory(command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var created = Assert.IsAssignableFrom<ShowCarInventoryDto>(okResult.Value);
            Assert.Equal(command.CarModelId, created.CarModelId);
        }


        [Fact]
        public async Task EditCarInventory_ReturnsOkResult_WhenUpdated()
        {
            // Arrange
            var command = new EditCarInventoryCommand
            {
                Quantity = 20,
                IdempotencyKey = Guid.NewGuid().ToString()
            };

            var fakeDto = new ShowCarInventoryDto
            {
                InventoryId = command.InventoryId,
                CarModelId = Guid.NewGuid(),
                CarModel = new ShowCarModelDto
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
                    CarPrice = 3500000,
                    CarModelId = Guid.NewGuid()
                },
                Quantity = (uint)command.Quantity!,
                LastUpdated = DateTime.UtcNow
            };

            var result = MbResult<ShowCarInventoryDto>.Success(fakeDto);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<EditCarInventoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var response = await _controller.EditCarInventory(Guid.NewGuid(),"idempotencyKey",command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var updated = Assert.IsAssignableFrom<ShowCarInventoryDto>(okResult.Value);
            Assert.Equal(command.Quantity, (int)updated.Quantity);
        }


        [Fact]
        public async Task DeleteCarInventory_ReturnsOkResult_WhenDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var fakeDto = new ShowCarInventoryDto
            {
                InventoryId = id,
                CarModelId = Guid.NewGuid(),
                CarModel = new ShowCarModelDto
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
                    CarPrice = 3500000,
                    CarModelId = Guid.NewGuid()
                },

                Quantity = 0,
                LastUpdated = DateTime.UtcNow
            };

            var result = MbResult<ShowCarInventoryDto>.Success(fakeDto);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<DeleteCarInventoryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var response = await _controller.DeleteCarInventory(id, Guid.NewGuid().ToString());

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var deleted = Assert.IsAssignableFrom<ShowCarInventoryDto>(okResult.Value);
            Assert.Equal(id, deleted.InventoryId);
        }
    }
}
