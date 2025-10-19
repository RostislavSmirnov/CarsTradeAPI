using CarsTradeAPI.Entities.Elements;
using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using CarsTradeAPI.Features.BuyerOperation.CreateBuyer;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;
using CarsTradeAPI.Features.CarInventoryOperation.CreateCarInventory;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Features.CarModelOperation.CreateCarModel;
using CarsTradeAPI.Features.EmployeeOperation.CreateEmployee;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Features.OrdersOperation.CreateOrder;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using CarsTradeTests.IntegrationTests.TestData;
using System.Net.Http.Json;
using CarsTradeAPI.Features.OrdersOperation.OrderItems.AddOrderItem;
using FluentAssertions;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using System.Net;
using CarsTradeAPI.Features.OrdersOperation.OrderItems.DeleteOrderItems;
using CarsTradeAPI.Features.OrdersOperation.OrderItems.EditOrderItem;
using CarsTradeAPI.Features.OrdersOperation.OrderItems.AddOrderItems;


namespace CarsTradeTests.IntegrationTests.OrderItemIntegrationTests
{
    [Collection("IntegrationTests")]
    public class OrderItemIntegrationTests : IntegrationTestBase
    {
        public OrderItemIntegrationTests(PostgreSqlContainerFixture dbFixture, CustomWebApplicationFactoryFixture factoryFixture)
            : base(dbFixture, factoryFixture) { }

        [Fact]
        public async Task CreateOrderItem_ShouldReturnOrder_WhenCommandValid()
        {
            CreateBuyerCommand buyerCommand = new CreateBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerName = "Ivan",
                BuyerSurname = "Petrov",
                BuyerMiddlename = "Sergeevich",
                BuyerEmail = "ivan.petrov@example.com",
                PhoneNumber = "+79161234567",
                BuyerAddress = "Moscow, Red Square"
            };
            HttpResponseMessage buyerPostResponse = await _client.PostAsJsonAsync("/api/Buyer/CreateBuyer", buyerCommand);
            buyerPostResponse.EnsureSuccessStatusCode();
            ShowBuyerDto? buyerCommandResult = await buyerPostResponse.Content.ReadFromJsonAsync<ShowBuyerDto>();

            CreateEmployeeCommand employeeCommand = new CreateEmployeeCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                EmployeeName = "Test",
                EmployeeSurname = "User",
                EmployeeMiddlename = "Integration",
                EmployeeAge = 30,
                EmployeeLogin = "test_login",
                EmployeePassword = "TestPass123!",
                EmployeeRole = "Admin"
            };
            HttpResponseMessage employePostResponse = await _client.PostAsJsonAsync("/api/Employee/CreateEmployee", employeeCommand);
            string responseContent = await employePostResponse.Content.ReadAsStringAsync();
            employePostResponse.EnsureSuccessStatusCode();
            ShowEmployeeDto? employeeCommandResult = await employePostResponse.Content.ReadFromJsonAsync<ShowEmployeeDto>();

            CreateCarModelCommand carModelCommand = new CreateCarModelCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                CarManufacturer = "Toyota",
                CarModelName = "Camry",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    WheelSize = 19.3F,
                    CarMusicBrand = "Sony"
                },
                CarCountry = "Japan",
                ProductionDateTime = DateTime.UtcNow,
                CarEngine = new Engine
                {
                    FuelType = "Gasoline",
                    EngineCapacity = 2.5F,
                    EngineHorsePower = 203
                },
                CarPrice = 3000000,
                CarColor = "White",
            };
            HttpResponseMessage carModelPostResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carModelCommand);
            carModelPostResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? carModelCommandResult = await carModelPostResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarInventoryCommand carInventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = carModelCommandResult!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", carInventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            CreateOrderCommand orderCommand = new CreateOrderCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerId = buyerCommandResult!.BuyerId,
                EmployeeId = employeeCommandResult!.EmployeeId,
                OrderCompletionDate = DateTime.UtcNow.AddDays(7),
                OrderAddress = new OrderAddress
                {
                    Country = "Russia",
                    City = "Moscow",
                    Region = "Moscow",
                    Street = "Lenina",
                },
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        CarModelId = carModelCommandResult!.CarModelId,
                        OrderQuantity = 2,
                        OrderItemComments = "No comments"
                    }
                }
            };
            HttpResponseMessage orderPostResponse = await _client.PostAsJsonAsync("/api/Orders/CreateOrder", orderCommand);
            orderPostResponse.EnsureSuccessStatusCode();
            ShowOrderDto? orderCommandResult = await orderPostResponse.Content.ReadFromJsonAsync<ShowOrderDto>();

            CreateOrderItemCommand addOrderItemCommand = new CreateOrderItemCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                OrderId = orderCommandResult!.OrderId,
                CarModelId = carModelCommandResult!.CarModelId,
                OrderQuantity = 1,
                OrderItemComments = "Additional item"
            };
            HttpResponseMessage addOrderItemResponse = await _client.PostAsJsonAsync("/api/OrderItems/AddOrderItem", addOrderItemCommand);
            string responseContentItem = await addOrderItemResponse.Content.ReadAsStringAsync();
            ShowOrderDto? addedOrderItem = await addOrderItemResponse.Content.ReadFromJsonAsync<ShowOrderDto>();

            Assert.Equal(HttpStatusCode.OK, addOrderItemResponse.StatusCode);
            Assert.NotNull(addedOrderItem);
            Assert.Equal(addOrderItemCommand.OrderId, addedOrderItem!.OrderId);
            Assert.Contains(addedOrderItem.OrderItems, oi => oi.OrderItemComments == "Additional item" && oi.OrderQuantity == 1);
            addedOrderItem.OrderItems.Should().ContainEquivalentOf(new ShowOrderItemDto
            {
                OrderItemComments = "Additional item",
                OrderQuantity = 1,
                OrderId = addOrderItemCommand.OrderId,
                UnitPrice = carModelCommandResult.CarPrice
            },
            options => options.Excluding(o => o.OrderItemId));
        }


        [Fact]
        public async Task CreateOrderItem_ShouldReturnNotFound_WhenOrderNotExists()
        {
            CreateBuyerCommand buyerCommand = new CreateBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerName = "Ivan",
                BuyerSurname = "Petrov",
                BuyerMiddlename = "Sergeevich",
                BuyerEmail = "ivan.petrov@example.com",
                PhoneNumber = "+79161234567",
                BuyerAddress = "Moscow, Red Square"
            };
            HttpResponseMessage buyerPostResponse = await _client.PostAsJsonAsync("/api/Buyer/CreateBuyer", buyerCommand);
            buyerPostResponse.EnsureSuccessStatusCode();
            ShowBuyerDto? buyerCommandResult = await buyerPostResponse.Content.ReadFromJsonAsync<ShowBuyerDto>();

            CreateEmployeeCommand employeeCommand = new CreateEmployeeCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                EmployeeName = "Test",
                EmployeeSurname = "User",
                EmployeeMiddlename = "Integration",
                EmployeeAge = 30,
                EmployeeLogin = "test_login",
                EmployeePassword = "TestPass123!",
                EmployeeRole = "Admin"
            };
            HttpResponseMessage employePostResponse = await _client.PostAsJsonAsync("/api/Employee/CreateEmployee", employeeCommand);
            string responseContent = await employePostResponse.Content.ReadAsStringAsync();
            employePostResponse.EnsureSuccessStatusCode();
            ShowEmployeeDto? employeeCommandResult = await employePostResponse.Content.ReadFromJsonAsync<ShowEmployeeDto>();

            CreateCarModelCommand carModelCommand = new CreateCarModelCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                CarManufacturer = "Toyota",
                CarModelName = "Camry",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    WheelSize = 19.3F,
                    CarMusicBrand = "Sony"
                },
                CarCountry = "Japan",
                ProductionDateTime = DateTime.UtcNow,
                CarEngine = new Engine
                {
                    FuelType = "Gasoline",
                    EngineCapacity = 2.5F,
                    EngineHorsePower = 203
                },
                CarPrice = 3000000,
                CarColor = "White",
            };
            HttpResponseMessage carModelPostResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carModelCommand);
            carModelPostResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? carModelCommandResult = await carModelPostResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarInventoryCommand carInventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = carModelCommandResult!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", carInventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            CreateOrderCommand orderCommand = new CreateOrderCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerId = buyerCommandResult!.BuyerId,
                EmployeeId = employeeCommandResult!.EmployeeId,
                OrderCompletionDate = DateTime.UtcNow.AddDays(7),
                OrderAddress = new OrderAddress
                {
                    Country = "Russia",
                    City = "Moscow",
                    Region = "Moscow",
                    Street = "Lenina",
                },
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        CarModelId = carModelCommandResult!.CarModelId,
                        OrderQuantity = 2,
                        OrderItemComments = "No comments"
                    }
                }
            };
            HttpResponseMessage orderPostResponse = await _client.PostAsJsonAsync("/api/Orders/CreateOrder", orderCommand);
            orderPostResponse.EnsureSuccessStatusCode();
            ShowOrderDto? orderCommandResult = await orderPostResponse.Content.ReadFromJsonAsync<ShowOrderDto>();

            CreateOrderItemCommand addOrderItemCommand = new CreateOrderItemCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                OrderId = Guid.NewGuid(),
                CarModelId = carModelCommandResult!.CarModelId,
                OrderQuantity = 1,
                OrderItemComments = "Additional item"
            };
            HttpResponseMessage addOrderItemResponse = await _client.PostAsJsonAsync("/api/OrderItems/AddOrderItem", addOrderItemCommand);
            string responseContentItem = await addOrderItemResponse.Content.ReadAsStringAsync();
            List<ErrorDetail>? addedOrderItem = await addOrderItemResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();
            Assert.Equal(HttpStatusCode.NotFound, addOrderItemResponse.StatusCode);
            Assert.Contains(addedOrderItem!, e => e.Code == "NOT_FOUND");
        }


        [Fact]
        public async Task CreateOrderItems_ShouldReturnNotFound_WhenCarModelNotExists()
        {
            CreateBuyerCommand buyerCommand = new CreateBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerName = "Ivan",
                BuyerSurname = "Petrov",
                BuyerMiddlename = "Sergeevich",
                BuyerEmail = "ivan.petrov@example.com",
                PhoneNumber = "+79161234567",
                BuyerAddress = "Moscow, Red Square"
            };
            HttpResponseMessage buyerPostResponse = await _client.PostAsJsonAsync("/api/Buyer/CreateBuyer", buyerCommand);
            buyerPostResponse.EnsureSuccessStatusCode();
            ShowBuyerDto? buyerCommandResult = await buyerPostResponse.Content.ReadFromJsonAsync<ShowBuyerDto>();

            CreateEmployeeCommand employeeCommand = new CreateEmployeeCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                EmployeeName = "Test",
                EmployeeSurname = "User",
                EmployeeMiddlename = "Integration",
                EmployeeAge = 30,
                EmployeeLogin = "test_login",
                EmployeePassword = "TestPass123!",
                EmployeeRole = "Admin"
            };
            HttpResponseMessage employePostResponse = await _client.PostAsJsonAsync("/api/Employee/CreateEmployee", employeeCommand);
            string responseContent = await employePostResponse.Content.ReadAsStringAsync();
            employePostResponse.EnsureSuccessStatusCode();
            ShowEmployeeDto? employeeCommandResult = await employePostResponse.Content.ReadFromJsonAsync<ShowEmployeeDto>();

            CreateCarModelCommand carModelCommand = new CreateCarModelCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                CarManufacturer = "Toyota",
                CarModelName = "Camry",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    WheelSize = 19.3F,
                    CarMusicBrand = "Sony"
                },
                CarCountry = "Japan",
                ProductionDateTime = DateTime.UtcNow,
                CarEngine = new Engine
                {
                    FuelType = "Gasoline",
                    EngineCapacity = 2.5F,
                    EngineHorsePower = 203
                },
                CarPrice = 3000000,
                CarColor = "White",
            };
            HttpResponseMessage carModelPostResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carModelCommand);
            carModelPostResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? carModelCommandResult = await carModelPostResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarModelCommand carModelCommand1 = new CreateCarModelCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                CarManufacturer = "Lexus",
                CarModelName = "LFA",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    WheelSize = 19.3F,
                    CarMusicBrand = "Sony"
                },
                CarCountry = "Japan",
                ProductionDateTime = DateTime.UtcNow,
                CarEngine = new Engine
                {
                    FuelType = "Gasoline",
                    EngineCapacity = 8,
                    EngineHorsePower = 650
                },
                CarPrice = 30000000,
                CarColor = "White",
            };
            HttpResponseMessage carModelPostResponse1 = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carModelCommand1);
            carModelPostResponse1.EnsureSuccessStatusCode();
            ShowCarModelDto? carModelCommandResult1 = await carModelPostResponse1.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarInventoryCommand carInventoryCommand1 = new CreateCarInventoryCommand
            {
                CarModelId = carModelCommandResult1!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse1 = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", carInventoryCommand1);
            inventoryResponse1.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory1 = await inventoryResponse1.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            CreateCarInventoryCommand carInventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = carModelCommandResult!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", carInventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            CreateOrderCommand orderCommand = new CreateOrderCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerId = buyerCommandResult!.BuyerId,
                EmployeeId = employeeCommandResult!.EmployeeId,
                OrderCompletionDate = DateTime.UtcNow.AddDays(7),
                OrderAddress = new OrderAddress
                {
                    Country = "Russia",
                    City = "Moscow",
                    Region = "Moscow",
                    Street = "Lenina",
                },
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        CarModelId = carModelCommandResult!.CarModelId,
                        OrderQuantity = 2,
                        OrderItemComments = "No comments"
                    }
                }
            };
            HttpResponseMessage orderPostResponse = await _client.PostAsJsonAsync("/api/Orders/CreateOrder", orderCommand);
            orderPostResponse.EnsureSuccessStatusCode();
            ShowOrderDto? orderCommandResult = await orderPostResponse.Content.ReadFromJsonAsync<ShowOrderDto>();

            AddOrderItemsCommand addOrderItemsCommand = new AddOrderItemsCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                OrderId = orderCommandResult!.OrderId,
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        CarModelId = carModelCommandResult!.CarModelId,
                        OrderQuantity = 1,
                        OrderItemComments = "Additional item"
                    },
                    new OrderItemDto
                    {
                        CarModelId = Guid.NewGuid(),
                        OrderQuantity = 2,
                        OrderItemComments = "Second additional item"
                    }
                }
            };
            HttpResponseMessage addOrderItemsResponse = await _client.PostAsJsonAsync("/api/OrderItems/AddOrderItems", addOrderItemsCommand);
            string responseContentItem = await addOrderItemsResponse.Content.ReadAsStringAsync();
            List<ErrorDetail>? addedOrderItemsErrors = await addOrderItemsResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();

            Assert.Equal(HttpStatusCode.NotFound, addOrderItemsResponse.StatusCode);
            Assert.NotNull(addedOrderItemsErrors);
            Assert.Contains(addedOrderItemsErrors!, e => e.Code == "NOT_FOUND");
        }


        [Fact]
        public async Task DeleteOrderItem_ShouldReturnUpdatedOrder_WhenCommandValid()
        {
            CreateBuyerCommand buyerCommand = new CreateBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerName = "Ivan",
                BuyerSurname = "Petrov",
                BuyerMiddlename = "Sergeevich",
                BuyerEmail = "ivan.petrov@example.com",
                PhoneNumber = "+79161234567",
                BuyerAddress = "Moscow, Red Square"
            };
            HttpResponseMessage buyerPostResponse = await _client.PostAsJsonAsync("/api/Buyer/CreateBuyer", buyerCommand);
            buyerPostResponse.EnsureSuccessStatusCode();
            ShowBuyerDto? buyerCommandResult = await buyerPostResponse.Content.ReadFromJsonAsync<ShowBuyerDto>();

            CreateEmployeeCommand employeeCommand = new CreateEmployeeCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                EmployeeName = "Test",
                EmployeeSurname = "User",
                EmployeeMiddlename = "Integration",
                EmployeeAge = 30,
                EmployeeLogin = "test_login",
                EmployeePassword = "TestPass123!",
                EmployeeRole = "Admin"
            };
            HttpResponseMessage employePostResponse = await _client.PostAsJsonAsync("/api/Employee/CreateEmployee", employeeCommand);
            string responseContent = await employePostResponse.Content.ReadAsStringAsync();
            employePostResponse.EnsureSuccessStatusCode();
            ShowEmployeeDto? employeeCommandResult = await employePostResponse.Content.ReadFromJsonAsync<ShowEmployeeDto>();

            CreateCarModelCommand carModelCommand = new CreateCarModelCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                CarManufacturer = "Toyota",
                CarModelName = "Camry",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    WheelSize = 19.3F,
                    CarMusicBrand = "Sony"
                },
                CarCountry = "Japan",
                ProductionDateTime = DateTime.UtcNow,
                CarEngine = new Engine
                {
                    FuelType = "Gasoline",
                    EngineCapacity = 2.5F,
                    EngineHorsePower = 203
                },
                CarPrice = 3000000,
                CarColor = "White",
            };
            HttpResponseMessage carModelPostResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carModelCommand);
            carModelPostResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? carModelCommandResult = await carModelPostResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarModelCommand carModelCommand1 = new CreateCarModelCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                CarManufacturer = "Subaru",
                CarModelName = "Forester",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    WheelSize = 19.3F,
                    CarMusicBrand = "Sony"
                },
                CarCountry = "Japan",
                ProductionDateTime = DateTime.UtcNow,
                CarEngine = new Engine
                {
                    FuelType = "Gasoline",
                    EngineCapacity = 2.5F,
                    EngineHorsePower = 203
                },
                CarPrice = 3000000,
                CarColor = "Blue",
            };
            HttpResponseMessage carModelPostResponse1 = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carModelCommand1);
            carModelPostResponse1.EnsureSuccessStatusCode();
            ShowCarModelDto? carModelCommandResult1 = await carModelPostResponse1.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarInventoryCommand carInventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = carModelCommandResult!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", carInventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            CreateCarInventoryCommand carInventoryCommand1 = new CreateCarInventoryCommand
            {
                CarModelId = carModelCommandResult1!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse1 = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", carInventoryCommand1);
            inventoryResponse1.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory1 = await inventoryResponse1.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            CreateOrderCommand orderCommand = new CreateOrderCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerId = buyerCommandResult!.BuyerId,
                EmployeeId = employeeCommandResult!.EmployeeId,
                OrderCompletionDate = DateTime.UtcNow.AddDays(7),
                OrderAddress = new OrderAddress
                {
                    Country = "Russia",
                    City = "Moscow",
                    Region = "Moscow",
                    Street = "Lenina",
                },
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        CarModelId = carModelCommandResult!.CarModelId,
                        OrderQuantity = 2,
                        OrderItemComments = "Item Toyota"
                    },
                    new OrderItemDto
                    {
                        CarModelId = carModelCommandResult1!.CarModelId,
                        OrderQuantity = 3,
                        OrderItemComments = "ItemSubaru"
                    }
                }
            };
            HttpResponseMessage orderPostResponse = await _client.PostAsJsonAsync("/api/Orders/CreateOrder", orderCommand);
            orderPostResponse.EnsureSuccessStatusCode();
            ShowOrderDto? orderCommandResult = await orderPostResponse.Content.ReadFromJsonAsync<ShowOrderDto>();

            DeleteOrderItemsCommand deleteOrderItemsCommand = new DeleteOrderItemsCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                OrderId = orderCommandResult!.OrderId,
                OrderItemId = new List<Guid>
                {
                    orderCommandResult.OrderItems[1].OrderItemId
                }
            };
            HttpRequestMessage deleteOrderItemResponse = new HttpRequestMessage(HttpMethod.Delete, $"/api/OrderItems/DeleteOrderItems");
            deleteOrderItemResponse.Content = JsonContent.Create(deleteOrderItemsCommand);
            HttpResponseMessage response = await _client.SendAsync(deleteOrderItemResponse);
            ShowOrderDto? updatedOrder = await response.Content.ReadFromJsonAsync<ShowOrderDto>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            updatedOrder.Should().NotBeNull();
            updatedOrder!.OrderId.Should().Be(deleteOrderItemsCommand.OrderId);
            updatedOrder.OrderItems.Should().HaveCount(1);
        }


        [Fact]
        public async Task DeleteOrderItem_ShouldReturnNotFound_WhenOrderItemnotFound()
        {
            CreateBuyerCommand buyerCommand = new CreateBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerName = "Ivan",
                BuyerSurname = "Petrov",
                BuyerMiddlename = "Sergeevich",
                BuyerEmail = "ivan.petrov@example.com",
                PhoneNumber = "+79161234567",
                BuyerAddress = "Moscow, Red Square"
            };
            HttpResponseMessage buyerPostResponse = await _client.PostAsJsonAsync("/api/Buyer/CreateBuyer", buyerCommand);
            buyerPostResponse.EnsureSuccessStatusCode();
            ShowBuyerDto? buyerCommandResult = await buyerPostResponse.Content.ReadFromJsonAsync<ShowBuyerDto>();

            CreateEmployeeCommand employeeCommand = new CreateEmployeeCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                EmployeeName = "Test",
                EmployeeSurname = "User",
                EmployeeMiddlename = "Integration",
                EmployeeAge = 30,
                EmployeeLogin = "test_login",
                EmployeePassword = "TestPass123!",
                EmployeeRole = "Admin"
            };
            HttpResponseMessage employePostResponse = await _client.PostAsJsonAsync("/api/Employee/CreateEmployee", employeeCommand);
            string responseContent = await employePostResponse.Content.ReadAsStringAsync();
            employePostResponse.EnsureSuccessStatusCode();
            ShowEmployeeDto? employeeCommandResult = await employePostResponse.Content.ReadFromJsonAsync<ShowEmployeeDto>();

            CreateCarModelCommand carModelCommand = new CreateCarModelCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                CarManufacturer = "Toyota",
                CarModelName = "Camry",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    WheelSize = 19.3F,
                    CarMusicBrand = "Sony"
                },
                CarCountry = "Japan",
                ProductionDateTime = DateTime.UtcNow,
                CarEngine = new Engine
                {
                    FuelType = "Gasoline",
                    EngineCapacity = 2.5F,
                    EngineHorsePower = 203
                },
                CarPrice = 3000000,
                CarColor = "White",
            };
            HttpResponseMessage carModelPostResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carModelCommand);
            carModelPostResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? carModelCommandResult = await carModelPostResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarModelCommand carModelCommand1 = new CreateCarModelCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                CarManufacturer = "Subaru",
                CarModelName = "Forester",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    WheelSize = 19.3F,
                    CarMusicBrand = "Sony"
                },
                CarCountry = "Japan",
                ProductionDateTime = DateTime.UtcNow,
                CarEngine = new Engine
                {
                    FuelType = "Gasoline",
                    EngineCapacity = 2.5F,
                    EngineHorsePower = 203
                },
                CarPrice = 3000000,
                CarColor = "Blue",
            };
            HttpResponseMessage carModelPostResponse1 = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carModelCommand1);
            carModelPostResponse1.EnsureSuccessStatusCode();
            ShowCarModelDto? carModelCommandResult1 = await carModelPostResponse1.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarInventoryCommand carInventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = carModelCommandResult!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", carInventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            CreateCarInventoryCommand carInventoryCommand1 = new CreateCarInventoryCommand
            {
                CarModelId = carModelCommandResult1!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse1 = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", carInventoryCommand1);
            inventoryResponse1.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory1 = await inventoryResponse1.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            CreateOrderCommand orderCommand = new CreateOrderCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerId = buyerCommandResult!.BuyerId,
                EmployeeId = employeeCommandResult!.EmployeeId,
                OrderCompletionDate = DateTime.UtcNow.AddDays(7),
                OrderAddress = new OrderAddress
                {
                    Country = "Russia",
                    City = "Moscow",
                    Region = "Moscow",
                    Street = "Lenina",
                },
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        CarModelId = carModelCommandResult!.CarModelId,
                        OrderQuantity = 2,
                        OrderItemComments = "Item Toyota"
                    },
                    new OrderItemDto
                    {
                        CarModelId = carModelCommandResult1!.CarModelId,
                        OrderQuantity = 3,
                        OrderItemComments = "ItemSubaru"
                    }
                }
            };
            HttpResponseMessage orderPostResponse = await _client.PostAsJsonAsync("/api/Orders/CreateOrder", orderCommand);
            orderPostResponse.EnsureSuccessStatusCode();
            ShowOrderDto? orderCommandResult = await orderPostResponse.Content.ReadFromJsonAsync<ShowOrderDto>();

            DeleteOrderItemsCommand deleteOrderItemsCommand = new DeleteOrderItemsCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                OrderId = orderCommandResult!.OrderId,
                OrderItemId = new List<Guid>
                {
                    Guid.NewGuid()
                }
            };
            HttpRequestMessage deleteOrderItemResponse = new HttpRequestMessage(HttpMethod.Delete, $"/api/OrderItems/DeleteOrderItems");
            deleteOrderItemResponse.Content = JsonContent.Create(deleteOrderItemsCommand);
            HttpResponseMessage response = await _client.SendAsync(deleteOrderItemResponse);
            List<ErrorDetail>? updatedOrder = await response.Content.ReadFromJsonAsync<List<ErrorDetail>>();
            updatedOrder.Should().NotBeNull();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            updatedOrder.Should().ContainSingle(e => e.Code == "NOT_FOUND");
        }


        [Fact]
        public async Task EditOrderItem_ShouldReturnOrder_WhenCommandValid()
        {
            CreateBuyerCommand buyerCommand = new CreateBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerName = "Ivan",
                BuyerSurname = "Petrov",
                BuyerMiddlename = "Sergeevich",
                BuyerEmail = "ivan.petrov@example.com",
                PhoneNumber = "+79161234567",
                BuyerAddress = "Moscow, Red Square"
            };
            HttpResponseMessage buyerPostResponse = await _client.PostAsJsonAsync("/api/Buyer/CreateBuyer", buyerCommand);
            buyerPostResponse.EnsureSuccessStatusCode();
            ShowBuyerDto? buyerCommandResult = await buyerPostResponse.Content.ReadFromJsonAsync<ShowBuyerDto>();

            CreateEmployeeCommand employeeCommand = new CreateEmployeeCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                EmployeeName = "Test",
                EmployeeSurname = "User",
                EmployeeMiddlename = "Integration",
                EmployeeAge = 30,
                EmployeeLogin = "test_login",
                EmployeePassword = "TestPass123!",
                EmployeeRole = "Admin"
            };
            HttpResponseMessage employePostResponse = await _client.PostAsJsonAsync("/api/Employee/CreateEmployee", employeeCommand);
            string responseContent = await employePostResponse.Content.ReadAsStringAsync();
            employePostResponse.EnsureSuccessStatusCode();
            ShowEmployeeDto? employeeCommandResult = await employePostResponse.Content.ReadFromJsonAsync<ShowEmployeeDto>();

            CreateCarModelCommand carModelCommand = new CreateCarModelCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                CarManufacturer = "Toyota",
                CarModelName = "Camry",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    WheelSize = 19.3F,
                    CarMusicBrand = "Sony"
                },
                CarCountry = "Japan",
                ProductionDateTime = DateTime.UtcNow,
                CarEngine = new Engine
                {
                    FuelType = "Gasoline",
                    EngineCapacity = 2.5F,
                    EngineHorsePower = 203
                },
                CarPrice = 3000000,
                CarColor = "White",
            };
            HttpResponseMessage carModelPostResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carModelCommand);
            carModelPostResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? carModelCommandResult = await carModelPostResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarInventoryCommand carInventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = carModelCommandResult!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", carInventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            CreateOrderCommand orderCommand = new CreateOrderCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerId = buyerCommandResult!.BuyerId,
                EmployeeId = employeeCommandResult!.EmployeeId,
                OrderCompletionDate = DateTime.UtcNow.AddDays(7),
                OrderAddress = new OrderAddress
                {
                    Country = "Russia",
                    City = "Moscow",
                    Region = "Moscow",
                    Street = "Lenina",
                },
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        CarModelId = carModelCommandResult!.CarModelId,
                        OrderQuantity = 2,
                        OrderItemComments = "No comments"
                    }
                }
            };
            HttpResponseMessage orderPostResponse = await _client.PostAsJsonAsync("/api/Orders/CreateOrder", orderCommand);
            orderPostResponse.EnsureSuccessStatusCode();
            ShowOrderDto? orderCommandResult = await orderPostResponse.Content.ReadFromJsonAsync<ShowOrderDto>();

            EditOrderItemCommand editOrderItemCommand = new EditOrderItemCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                OrderId = orderCommandResult!.OrderId,
                OrderItemId = orderCommandResult!.OrderItems[0].OrderItemId,
                OrderQuantity = 5,
                OrderItemComments = "Updated comments"
            };
            HttpResponseMessage editOrderItemResponse = await _client.PutAsJsonAsync("/api/OrderItems/EditOrderItem", editOrderItemCommand);
            string responseContentItem = await editOrderItemResponse.Content.ReadAsStringAsync();
            ShowOrderDto? updatedOrder = await editOrderItemResponse.Content.ReadFromJsonAsync<ShowOrderDto>();

            Assert.Equal(HttpStatusCode.OK, editOrderItemResponse.StatusCode);
            Assert.NotNull(updatedOrder);
            Assert.Equal(editOrderItemCommand.OrderItemId, updatedOrder!.OrderItems[0].OrderItemId);
            Assert.Equal((int)editOrderItemCommand.OrderQuantity, (int)updatedOrder.OrderItems[0].OrderQuantity);
        }


        [Fact]
        public async Task EditOrderItem_ShouldReturnNotFound_WhenCommandInvalid()
        {
            CreateBuyerCommand buyerCommand = new CreateBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerName = "Ivan",
                BuyerSurname = "Petrov",
                BuyerMiddlename = "Sergeevich",
                BuyerEmail = "ivan.petrov@example.com",
                PhoneNumber = "+79161234567",
                BuyerAddress = "Moscow, Red Square"
            };
            HttpResponseMessage buyerPostResponse = await _client.PostAsJsonAsync("/api/Buyer/CreateBuyer", buyerCommand);
            buyerPostResponse.EnsureSuccessStatusCode();
            ShowBuyerDto? buyerCommandResult = await buyerPostResponse.Content.ReadFromJsonAsync<ShowBuyerDto>();

            CreateEmployeeCommand employeeCommand = new CreateEmployeeCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                EmployeeName = "Test",
                EmployeeSurname = "User",
                EmployeeMiddlename = "Integration",
                EmployeeAge = 30,
                EmployeeLogin = "test_login",
                EmployeePassword = "TestPass123!",
                EmployeeRole = "Admin"
            };
            HttpResponseMessage employePostResponse = await _client.PostAsJsonAsync("/api/Employee/CreateEmployee", employeeCommand);
            string responseContent = await employePostResponse.Content.ReadAsStringAsync();
            employePostResponse.EnsureSuccessStatusCode();
            ShowEmployeeDto? employeeCommandResult = await employePostResponse.Content.ReadFromJsonAsync<ShowEmployeeDto>();

            CreateCarModelCommand carModelCommand = new CreateCarModelCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                CarManufacturer = "Toyota",
                CarModelName = "Camry",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    WheelSize = 19.3F,
                    CarMusicBrand = "Sony"
                },
                CarCountry = "Japan",
                ProductionDateTime = DateTime.UtcNow,
                CarEngine = new Engine
                {
                    FuelType = "Gasoline",
                    EngineCapacity = 2.5F,
                    EngineHorsePower = 203
                },
                CarPrice = 3000000,
                CarColor = "White",
            };
            HttpResponseMessage carModelPostResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carModelCommand);
            carModelPostResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? carModelCommandResult = await carModelPostResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarInventoryCommand carInventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = carModelCommandResult!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", carInventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            CreateOrderCommand orderCommand = new CreateOrderCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerId = buyerCommandResult!.BuyerId,
                EmployeeId = employeeCommandResult!.EmployeeId,
                OrderCompletionDate = DateTime.UtcNow.AddDays(7),
                OrderAddress = new OrderAddress
                {
                    Country = "Russia",
                    City = "Moscow",
                    Region = "Moscow",
                    Street = "Lenina",
                },
                OrderItems = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        CarModelId = carModelCommandResult!.CarModelId,
                        OrderQuantity = 2,
                        OrderItemComments = "No comments"
                    }
                }
            };
            HttpResponseMessage orderPostResponse = await _client.PostAsJsonAsync("/api/Orders/CreateOrder", orderCommand);
            orderPostResponse.EnsureSuccessStatusCode();
            ShowOrderDto? orderCommandResult = await orderPostResponse.Content.ReadFromJsonAsync<ShowOrderDto>();

            EditOrderItemCommand editOrderItemCommand = new EditOrderItemCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                OrderId = orderCommandResult!.OrderId,
                OrderItemId = orderCommandResult!.OrderItems[0].OrderItemId,
                OrderQuantity = -1,
                OrderItemComments = "Updated comments"
            };
            HttpResponseMessage editOrderItemResponse = await _client.PutAsJsonAsync("/api/OrderItems/EditOrderItem", editOrderItemCommand);
            string responseContentItem = await editOrderItemResponse.Content.ReadAsStringAsync();
            List<ErrorDetail>? updatedOrder = await editOrderItemResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();

            Assert.NotNull(updatedOrder);
            Assert.Equal(HttpStatusCode.BadRequest, editOrderItemResponse.StatusCode);
            Assert.Contains(updatedOrder, e => e.Code == "VALIDATION_ERROR");
        }
    }
}