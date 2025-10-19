using Moq;
using Microsoft.Extensions.Logging;
using MediatR;
using CarsTradeAPI.Features.BuyerOperation;
using CarsTradeAPI.Features.BuyerOperation.CreateBuyer;
using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using CarsTradeAPI.Features.BuyerOperation.GetBuyerById;
using CarsTradeAPI.Features.BuyerOperation.EditBuyer;
using CarsTradeAPI.Features.BuyerOperation.DeleteBuyer;


namespace CarsTradeTests.UnitTests.BuyerTests
{
    /// <summary>
    /// Тесты для контроллера BuyerController
    /// </summary>
    public class BuyerControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<BuyerController>> _loggerMock;
        private readonly BuyerController _controller;
        public BuyerControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<BuyerController>>();
            _controller = new BuyerController(_mediatorMock.Object, _loggerMock.Object);
        }


        [Fact]
        public async Task CreateBuyer_ShouldReturnOk_WhenBuyerCreatedSuccessfully() 
        {
            // Arrange
            var command = new CreateBuyerCommand
            {
                IdempotencyKey = "abc123",
                BuyerName = "John",
                BuyerSurname = "Doe",
                BuyerMiddlename = "Markovich",
                BuyerEmail = "john.doe@mail.com",
                PhoneNumber = "89210513934",
                BuyerAddress = "New York"
            };

            var expectedBuyer = new ShowBuyerDto
            {
                BuyerId = Guid.NewGuid(),
                BuyerName = "John",
                BuyerSurname = "Doe",
                BuyerMiddlename = "Markovich",
                BuyerEmail = "john.doe@mail.com",
                PhoneNumber = "89210513934",
                BuyerAddress = "New York"
            };

            MbResult<ShowBuyerDto> successResult = MbResult<ShowBuyerDto>.Success(expectedBuyer);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateBuyerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successResult);

            // Act
            var result = await _controller.CreateBuyer(command);

            // Asserts
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var value = okResult.Value as ShowBuyerDto;
            value.Should().NotBeNull();
            value!.BuyerName.Should().Be("John");
        }


        [Fact]
        public async Task CreateBuyer_ShouldReturnBadRequest_WhenBuyerCreationFails()
        {
            // Arrange
            var command = new CreateBuyerCommand
            {
                IdempotencyKey = "abc124",
                BuyerName = "Invalid",
                BuyerSurname = "User",
                BuyerMiddlename = "X",
                BuyerEmail = "invalid@mail.com",
                PhoneNumber = "000000",
                BuyerAddress = "Unknown"
            };

            var failure = MbResult<ShowBuyerDto>.Failure(new[]
            {
                new ErrorDetail("VALIDATION_ERROR", "Ошибка валидации при создании нового покупателя", "CreateBuyer")
            });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateBuyerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failure);

            // Act
            var result = await _controller.CreateBuyer(command);

            // Assert
            var badRequest = result.Result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.StatusCode.Should().Be(400);
        }


        [Fact]
        public async Task GetBuyerById_ShouldReturnOk_WhenBuyerExists()
        {
            // Arrange
            var buyerId = Guid.NewGuid();
            var expected = new ShowBuyerDto
            {
                BuyerId = buyerId,
                BuyerName = "Alex",
                BuyerSurname = "Smith",
                BuyerMiddlename = "Jimovich",
                BuyerEmail = "alex@mail.com",
                PhoneNumber = "89213456781",
                BuyerAddress = "Cherepovets"
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetBuyerByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MbResult<ShowBuyerDto>.Success(expected));

            // Act
            var result = await _controller.GetBuyerById(buyerId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var dto = okResult.Value as ShowBuyerDto;
            dto.Should().NotBeNull();
            dto!.BuyerName.Should().Be("Alex");
        }


        [Fact]
        public async Task GetBuyerById_ShouldReturnNotFound_WhenBuyerDoesNotExist()
        {
            // Arrange
            var buyerId = Guid.NewGuid();
            var failure = MbResult<ShowBuyerDto>.Failure(new[]
            {
                new ErrorDetail("NOT_FOUND", "Buyer not found", "GetBuyerById")
            });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetBuyerByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failure);

            // Act
            var result = await _controller.GetBuyerById(buyerId);

            // Assert
            var notFound = result.Result as NotFoundObjectResult;
            notFound.Should().NotBeNull();
            notFound!.StatusCode.Should().Be(404);
        }


        [Fact]
        public async Task EditBuyer_ShouldReturnOk_WhenBuyerEditedSuccessfully()
        {
            // Arrange
            var command = new EditBuyerCommand
            {
                BuyerId = Guid.NewGuid(),
                BuyerName = "John Updated",
                BuyerSurname = "Doe",
                BuyerEmail = "john.updated@mail.com"
            };

            var updatedBuyer = new ShowBuyerDto
            {
                BuyerId = command.BuyerId,
                BuyerName = "John Updated",
                BuyerSurname = "Doe",
                BuyerEmail = "john.updated@mail.com",
                BuyerMiddlename = "Markovich",
                PhoneNumber = "89210513932",
                BuyerAddress = "New York"
            };

            var success = MbResult<ShowBuyerDto>.Success(updatedBuyer);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<EditBuyerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(success);

            // Act
            var result = await _controller.EditBuyer(command);

            // Assert
            var ok = result.Result as OkObjectResult;
            ok.Should().NotBeNull();
            ok!.StatusCode.Should().Be(200);

            var dto = ok.Value as ShowBuyerDto;
            dto!.BuyerName.Should().Be("John Updated");
        }

       
        [Fact]
        public async Task EditBuyer_ShouldReturnBadRequest_WhenBuyerEditFails()
        {
            // Arrange
            var command = new EditBuyerCommand { BuyerId = Guid.NewGuid() };

            var fail = MbResult<ShowBuyerDto>.Failure(new[]
            {
                new ErrorDetail("VALIDATION_ERROR", "Buyer not found", "EditBuyer")
            });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<EditBuyerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fail);

            // Act
            var result = await _controller.EditBuyer(command);

            // Assert
            var bad = result.Result as BadRequestObjectResult;
            bad.Should().NotBeNull();
            bad!.StatusCode.Should().Be(400);
        }


        [Fact]
        public async Task DeleteBuyer_ShouldReturnOk_WhenBuyerDeletedSuccessfully()
        {
            // Arrange
            var buyerId = Guid.NewGuid();

            var deletedBuyer = new ShowBuyerDto
            {
                BuyerId = buyerId,
                BuyerName = "Ivan",
                BuyerSurname = "Petrov",
                BuyerMiddlename = "Sergeevich",
                BuyerEmail = "ivan.petrov@example.com",
                PhoneNumber = "+79998887766",
                BuyerAddress = "Moscow"
            };

            var success = MbResult<ShowBuyerDto>.Success(deletedBuyer);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<DeleteBuyerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(success);

            // Act
            var result = await _controller.DeleteBuyer(buyerId, "idempotencyKey");

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var value = okResult.Value as ShowBuyerDto;
            value.Should().NotBeNull();
            value!.BuyerId.Should().Be(buyerId);
            value.BuyerName.Should().Be("Ivan");
        }
    }
}

