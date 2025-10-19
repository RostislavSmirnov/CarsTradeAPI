using Moq;
using Microsoft.Extensions.Logging;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using CarsTradeAPI.Features.EmployeeOperation;
using CarsTradeAPI.Features.EmployeeOperation.CreateEmployee;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Features.EmployeeOperation.GetEmployeeById;
using CarsTradeAPI.Features.EmployeeOperation.EditEmployee;
using CarsTradeAPI.Features.EmployeeOperation.DeleteEmployee;


namespace CarsTradeTests.UnitTests.EmployeeTests
{
    /// <summary>
    /// Тесты для контроллера EmployeeController
    /// </summary>
    public class EmployeeControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<EmployeeController>> _loggerMock;
        private readonly EmployeeController _controller;
        public EmployeeControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<EmployeeController>>();
            _controller = new EmployeeController(_mediatorMock.Object, _loggerMock.Object);
        }


        [Fact]
        public async Task CreateEmployee_ShouldReturnOk_WhenBuyerCreatedSuccessfully()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                IdempotencyKey = "abc123",
                EmployeeName = "John",
                EmployeeSurname = "Doe",
                EmployeeMiddlename = "Markovich",
                EmployeeAge = 30,
                EmployeeRole = "Admin",
                EmployeeLogin = "johndoe",
                EmployeePassword = "password123"
            };

            var expectedEmployee = new ShowEmployeeDto
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeName = "John",
                EmployeeSurname = "Doe",
                EmployeeMiddlename = "Markovich",
                EmployeeAge = 30,
                EmployeeRole = "Admin",
                EmployeeSellCounter = 0,
            };

            // Act
            MbResult<ShowEmployeeDto> successResult = MbResult<ShowEmployeeDto>.Success(expectedEmployee);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateEmployeeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successResult);

            // Act
            var result = await _controller.CreateEmployee(command);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);

            var value = okResult.Value as ShowEmployeeDto;
            value.Should().NotBeNull();
            value!.EmployeeName.Should().Be(command.EmployeeName);
        }


        [Fact]
        public async Task CreateEmployee_ShouldReturnBadRequest_WhenMediatorReturnsError()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                IdempotencyKey = "abc123",
                EmployeeName = "John",
                EmployeeSurname = "Doe",
                EmployeeMiddlename = "X",
                EmployeeAge = 30,
                EmployeeRole = "Admin",
                EmployeeLogin = "johndoeLogin",
                EmployeePassword = "password123"
            };

            var failure = MbResult<ShowEmployeeDto>.Failure(new[]
            {
                new ErrorDetail("VALIDATION_ERROR", "Ошибка валидации при создании нового сотрудника","CreateEmployee")
            });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateEmployeeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failure);

            // Act
            var result = await _controller.CreateEmployee(command);
            
            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
        }


        [Fact]
        public async Task GetEmployeById_ShouldReturnOk_WhenBuyerExists()
        {
            // Arrange
            var employeeId = Guid.NewGuid();

            var expectedEmployee = new ShowEmployeeDto
            {
                EmployeeId = employeeId,
                EmployeeName = "Jane",
                EmployeeSurname = "Smith",
                EmployeeMiddlename = "A.",
                EmployeeAge = 28,
                EmployeeRole = "Sales",
                EmployeeSellCounter = 5,
            };

            MbResult<ShowEmployeeDto> successResult = MbResult<ShowEmployeeDto>.Success(expectedEmployee);
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetEmployeeByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successResult);
            // Act
            var result = await _controller.GetEmployeeById(employeeId);
            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var value = okResult.Value as ShowEmployeeDto;
            value.Should().NotBeNull();
            value!.EmployeeName.Should().Be("Jane");
        }


        [Fact]
        public async Task GetEmployeeById_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var failure = MbResult<ShowEmployeeDto>.Failure(new[]
            {
                new ErrorDetail("NOT_FOUND", "Сотрудник не найден", "GetEmployeeById")
            });
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetEmployeeByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failure);
            // Act
            var result = await _controller.GetEmployeeById(employeeId);
            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }


        [Fact]
        public async Task EditEmployee_ShouldReturnOk_WhenBuyerEditedSuccessfully() 
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var command = new EditEmployeeCommand
            {
                EmployeeId = employeeId,
                EmployeeName = "John",
                EmployeeSurname = "Doe",
                EmployeeMiddlename = "Markovich",
                EmployeeAge = 30,
                EmployeeRole = "Admin",
                EmployeeLogin = "johndoe",
                EmployeePassword = "password123"
            };

            var updatedEmployee = new ShowEmployeeDto
            {
                EmployeeId = employeeId,
                EmployeeName = "John Updated",
                EmployeeSurname = "Doe",
                EmployeeMiddlename = "Markovich",
                EmployeeAge = 31,
                EmployeeRole = "Admin",
                EmployeeSellCounter = 0,
            };

            var successResult = MbResult<ShowEmployeeDto>.Success(updatedEmployee);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<EditEmployeeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successResult);

            // Act
            var result = await _controller.EditEmployee(employeeId,"idempotencyKey", command);
        }


        [Fact]
        public async Task EditEmployee_ShouldReturnBadRequest_WhenBuyerEditFails() 
        {
            var employeeId = Guid.NewGuid();
            // Arrange
            var command = new EditEmployeeCommand
            {
                EmployeeId = employeeId,
                EmployeeName = "John",
                EmployeeSurname = "Doe",
                EmployeeMiddlename = "X",
                EmployeeAge = 30,
                EmployeeRole = "Admin",
                EmployeeLogin = "johndoeLogin",
                EmployeePassword = "password123"
            };
            var failureResult = MbResult<ShowEmployeeDto>.Failure(new[]
            {
                new ErrorDetail("VALIDATION_ERROR", "Ошибка валидации при редактировании сотрудника","EditEmployee")
            });
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<EditEmployeeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failureResult);
            // Act
            var result = await _controller.EditEmployee(employeeId,"IdempotencyKey", command);
            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
        }


        [Fact]
        public async Task DeleteEmployee_ShouldReturnOk_WhenEmployeeDeletedSuccessfuly() 
        {
            // Arrange
            var EmployeeId = Guid.NewGuid();

            var deletedEmployee = new ShowEmployeeDto
            {
                EmployeeId = EmployeeId,
                EmployeeName = "Jane",
                EmployeeSurname = "Smith",
                EmployeeMiddlename = "Adomovich",
                EmployeeAge = 28,
                EmployeeRole = "Admin",
                EmployeeSellCounter = 0,
            };

            var successResult = MbResult<ShowEmployeeDto>.Success(deletedEmployee);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<DeleteEmployeeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successResult);

            // Act
            var result = await _controller.DeleteEmployee(EmployeeId,"IdempotencyKey");

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var value = okResult.Value as ShowEmployeeDto;
            value.Should().NotBeNull();
            value!.EmployeeId.Should().Be(EmployeeId);
            value.EmployeeName.Should().Be("Jane");
        }
    }
}
