using Moq;
using Microsoft.Extensions.Logging;
using MediatR;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using CarsTradeAPI.Features.OrdersOperation;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeAPI.Features.OrdersOperation.CreateOrder;
using CarsTradeAPI.Entities.Elements;
using CarsTradeAPI.Features.OrdersOperation.GetOrderById;
using CarsTradeAPI.Features.OrdersOperation.EditOrder;
using CarsTradeAPI.Features.OrdersOperation.DeleteOrder;


namespace CarsTradeTests.UnitTests.OrderTests
{
    /// <summary>
    /// Класс для тестирования контроллера заказов
    /// </summary>
    public class OrderControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<OrdersController>> _loggerMock;
        private readonly OrdersController _orderController;
        public OrderControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<OrdersController>>();
            _orderController = new OrdersController(_mediatorMock.Object, _loggerMock.Object);
        }


        [Fact]
        public async Task CreateOrder_ShouldReturnOk_WhenOrderCreatedSuccessfully()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                IdempotencyKey = "abc123",
                OrderCompletionDate = DateTime.UtcNow.AddDays(7),
                BuyerId = Guid.NewGuid(),
                OrderAddress = new OrderAddress
                {
                    City = "Cherepovets",
                    Street = "Lenina",
                    Country = "Russia",
                    Region = "VologodskayaOblast",
                },
                EmployeeId = Guid.NewGuid(),
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        CarModelId = Guid.NewGuid(),
                        OrderItemComments = "No comments",
                        OrderQuantity = 1,
                    }
                }
            };

            var orderId = Guid.NewGuid();
            var expectedOrder = new ShowOrderDto
            {
                OrderId = orderId,
                OrderCreateDate = DateTime.UtcNow,
                OrderCompletionDate = command.OrderCompletionDate,
                BuyerId = command.BuyerId,
                EmployeeId = command.EmployeeId,
                OrderAddress = command.OrderAddress,
                OrderItems = new List<ShowOrderItemDto>
                {
                    new ShowOrderItemDto
                    {
                        OrderItemComments = "No comments",
                        OrderQuantity = 1,
                        OrderId = orderId,
                        OrderItemId = Guid.NewGuid(),
                        UnitPrice = 2500000,
                    }
                },
                OrderPrice = 2500000,
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MbResult<ShowOrderDto>.Success(expectedOrder));
            // Act
            var result = await _orderController.CreateOrder(command);
            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            var returnedOrder = okResult.Value as ShowOrderDto;
            returnedOrder.Should().NotBeNull();
            returnedOrder.Should().BeEquivalentTo(expectedOrder);
        }


        [Fact]
        public async Task CreateOrder_ShouldReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            var command = new CreateOrderCommand
            {
                IdempotencyKey = "abc123",
                OrderCompletionDate = DateTime.UtcNow.AddDays(7),
                BuyerId = Guid.NewGuid(),
                EmployeeId = Guid.NewGuid(),
                OrderAddress = null!,
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        CarModelId = Guid.NewGuid(),
                        OrderItemComments = "No comments",
                        OrderQuantity = 1,
                    }
                }
            };
            var errorDetails = new List<ErrorDetail>
            {
                new ErrorDetail("VALIDATION_ERROR", "Адрес не может быть пустым ", "CreateOrderCommand")
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MbResult<ShowOrderDto>.Failure(errorDetails));
            // Act
            var result = await _orderController.CreateOrder(command);
            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            var errors = badRequestResult.Value as IEnumerable<ErrorDetail>;
            errors.Should().NotBeNull();
            errors.Should().BeEquivalentTo(errorDetails);
        }


        [Fact]
        public async Task GetOrderById_ShouldReturnOk_WhenOrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var expectedOrder = new ShowOrderDto
            {
                OrderId = orderId,
                OrderCreateDate = DateTime.UtcNow,
                OrderCompletionDate = DateTime.UtcNow.AddDays(7),
                BuyerId = Guid.NewGuid(),
                EmployeeId = Guid.NewGuid(),
                OrderAddress = new OrderAddress
                {
                    City = "Cherepovets",
                    Street = "Lenina",
                    Country = "Russia",
                    Region = "VologodskayaOblast",
                },
                OrderItems = new List<ShowOrderItemDto>
                {
                    new ShowOrderItemDto
                    {
                        OrderItemComments = "No comments",
                        OrderQuantity = 1,
                        OrderId = orderId,
                        OrderItemId = Guid.NewGuid(),
                        UnitPrice = 2500000,
                    }
                },
                OrderPrice = 2500000,
            };
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MbResult<ShowOrderDto>.Success(expectedOrder));
            // Act
            var result = await _orderController.GetOrderById(orderId);
            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            var returnedOrder = okResult.Value as ShowOrderDto;
            returnedOrder.Should().NotBeNull();
            returnedOrder.Should().BeEquivalentTo(expectedOrder);
        }


        [Fact]
        public async Task GetOrderById_ShouldReturnNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var errorDetails = new List<ErrorDetail>
            {
                new ErrorDetail("NOT_FOUND", $"Заказ с ID: {orderId} не найден", "GetOrderByIdQuery")
            };
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MbResult<ShowOrderDto>.Failure(errorDetails));
            // Act
            var result = await _orderController.GetOrderById(orderId);
            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
            var errors = notFoundResult.Value as IEnumerable<ErrorDetail>;
            errors.Should().NotBeNull();
            errors.Should().BeEquivalentTo(errorDetails);
        }


        [Fact]
        public async Task EditOrder_ShouldReturnOk_WhenOrderEditedSuccesfully()
        {
            var orderId = Guid.NewGuid();
            var employeeId = Guid.NewGuid();
            var buyerId = Guid.NewGuid();

            var command = new EditOrderCommand
            {
                IdempotencyKey = "xyz123",
                OrderId = orderId,
                OrderCompletionDate = DateTime.UtcNow.AddDays(10),
                BuyerId = buyerId,
                EmployeeId = employeeId,
                OrderAddress = new OrderAddress
                {
                    City = "Cherepovets",
                    Street = "Lenina",
                    Country = "Russia",
                    Region = "VologodskayaOblast",
                },
            };

            var expectedOrder = new ShowOrderDto
            {
                OrderId = (Guid)command.OrderId,
                OrderCreateDate = DateTime.UtcNow,
                OrderCompletionDate = command.OrderCompletionDate,
                BuyerId = buyerId,
                EmployeeId = employeeId,
                OrderAddress = command.OrderAddress,
                OrderItems = new List<ShowOrderItemDto>
                {
                    new ShowOrderItemDto
                    {
                        OrderItemComments = "No comments",
                        OrderQuantity = 1,
                        OrderId = (Guid) command.OrderId,
                        OrderItemId = Guid.NewGuid(),
                        UnitPrice = 2500000,
                    }
                },
                OrderPrice = 2500000,
            };

            var result = MbResult<ShowOrderDto>.Success(expectedOrder);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<EditOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MbResult<ShowOrderDto>.Success(expectedOrder));

            // Act
            var okresult = await _orderController.EditOrder((Guid)command.OrderId,"idempotencyKey", command);

            // Assert
            var actionResult = okresult.Result as OkObjectResult;
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(200);
            var returnedOrder = actionResult.Value as ShowOrderDto;
            returnedOrder.Should().NotBeNull();
            returnedOrder.Should().BeEquivalentTo(expectedOrder);
        }


        [Fact]
        public async Task EditOrder_ShouldReturnBadRequest_WhenValidationFails()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var command = new EditOrderCommand
            {
                IdempotencyKey = "xyz123",
                OrderId = orderId,
                OrderCompletionDate = DateTime.UtcNow.AddDays(10),
                BuyerId = Guid.NewGuid(),
                EmployeeId = Guid.NewGuid(),
                OrderAddress = null!,
            };
            var errorDetails = new List<ErrorDetail>
            {
                new ErrorDetail("VALIDATION_ERROR", "Адрес не может быть пустым ", "EditOrderCommand")
            };
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<EditOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MbResult<ShowOrderDto>.Failure(errorDetails));
            // Act
            var result = await _orderController.EditOrder(orderId,"idempotencyKey", command);
            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult.StatusCode.Should().Be(400);
            var errors = badRequestResult.Value as IEnumerable<ErrorDetail>;
            errors.Should().NotBeNull();
            errors.Should().BeEquivalentTo(errorDetails);
        }


        [Fact]
        public async Task DeleteOrder_ShouldReturnOk_WhenOrderDeletedSuccesfully() 
        {
            var orderId = Guid.NewGuid();

            var expectedOrder = new ShowOrderDto
            {
                OrderId = orderId,
                OrderCreateDate = DateTime.UtcNow,
                OrderCompletionDate = DateTime.UtcNow.AddDays(7),
                BuyerId = Guid.NewGuid(),
                EmployeeId = Guid.NewGuid(),
                OrderAddress = new OrderAddress
                {
                    City = "Cherepovets",
                    Street = "Lenina",
                    Country = "Russia",
                    Region = "VologodskayaOblast",
                },
                OrderItems = new List<ShowOrderItemDto>
                {
                    new ShowOrderItemDto
                    {
                        OrderItemComments = "No comments",
                        OrderQuantity = 1,
                        OrderId = orderId,
                        OrderItemId = Guid.NewGuid(),
                        UnitPrice = 2500000,
                    }
                },
                OrderPrice = 2500000,
            };

            var successResult = MbResult<ShowOrderDto>.Success(expectedOrder);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<DeleteOrderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(successResult);

            // Act
            var result = await _orderController.DeleteOrder(orderId,"idempotencyKey");

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            var returnedOrder = okResult.Value as ShowOrderDto;
            returnedOrder.Should().NotBeNull();
            returnedOrder.Should().BeEquivalentTo(expectedOrder);
        }
    }
}
