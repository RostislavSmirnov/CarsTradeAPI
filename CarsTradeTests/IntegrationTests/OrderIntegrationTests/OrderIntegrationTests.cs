using CarsTradeAPI.Entities.Elements;
using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using CarsTradeAPI.Features.BuyerOperation.CreateBuyer;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;
using CarsTradeAPI.Features.CarInventoryOperation.CreateCarInventory;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Features.CarModelOperation.CreateCarModel;
using CarsTradeAPI.Features.EmployeeOperation.CreateEmployee;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeTests.IntegrationTests.TestData;
using CarsTradeAPI.Features.OrdersOperation.OrdersDto;
using System.Net.Http.Json;
using CarsTradeAPI.Features.OrdersOperation.CreateOrder;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using System.Net;
using CarsTradeAPI.Features.OrdersOperation.EditOrder;



namespace CarsTradeTests.IntegrationTests.OrderIntegrationTests
{
    [Collection("IntegrationTests")]
    public class OrderIntegrationTests : IntegrationTestBase
    {
        public OrderIntegrationTests(PostgreSqlContainerFixture dbFixture, CustomWebApplicationFactoryFixture factoryFixture)
            : base(dbFixture, factoryFixture) { }

        [Fact]
        public async Task CreateOrder_ShouldReturnOrder_WhenCommandValid()
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

            Assert.NotNull(orderCommandResult);
            Assert.Equal(orderCommand.BuyerId, orderCommandResult!.BuyerId);
            Assert.Equal(orderCommand.EmployeeId, orderCommandResult.EmployeeId);
            Assert.Equal(orderCommand.OrderCompletionDate, orderCommandResult.OrderCompletionDate);
            Assert.Equal(orderCommand.OrderAddress.City, orderCommandResult.OrderAddress.City);
        }


        [Fact]
        public async Task CreateOrder_ShouldReturnOkAndDeletedOrderDto_WhenCommandInvalid()
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
                        CarModelId = Guid.NewGuid(),
                        OrderQuantity = 2,
                        OrderItemComments = "No comments"
                    }
                }
            };
            HttpResponseMessage orderPostResponse = await _client.PostAsJsonAsync("/api/Orders/CreateOrder", orderCommand);
            List<ErrorDetail>? orderCommandResult = await orderPostResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();

            Assert.NotNull(orderCommandResult);
            Assert.Equal(HttpStatusCode.NotFound, orderPostResponse.StatusCode);
            Assert.Contains(orderCommandResult!, e => e.Code == "NOT_FOUND");
        }


        [Fact]
        public async Task DeleteOrder_ShouldReturnOrder_WhenCommandValid()
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

            HttpRequestMessage deleteRequestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/Orders/DeleteOrder/{orderCommandResult!.OrderId}");
            deleteRequestMessage.Headers.Add("idempotencyKey", Guid.NewGuid().ToString());
            HttpResponseMessage orderDeleteResponse = await _client.SendAsync(deleteRequestMessage);

            orderDeleteResponse.EnsureSuccessStatusCode();
            ShowOrderDto? deleteCommandResult = await orderDeleteResponse.Content.ReadFromJsonAsync<ShowOrderDto>();

            Assert.NotNull(deleteCommandResult);
            Assert.Equal(HttpStatusCode.OK, orderDeleteResponse.StatusCode);
            Assert.Equal(orderCommandResult.OrderId, deleteCommandResult!.OrderId);
            Assert.Equal(orderCommandResult.BuyerId, deleteCommandResult.BuyerId);
            Assert.Equal(orderCommandResult.EmployeeId, deleteCommandResult.EmployeeId);
        }


        [Fact]
        public async Task DeleteOrder_ShouldReturnNotFound_WhenOrderNotExists()
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

            HttpRequestMessage deleteRequestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/Orders/DeleteOrder/{Guid.NewGuid()}");
            deleteRequestMessage.Headers.Add("idempotencyKey", Guid.NewGuid().ToString());
            HttpResponseMessage orderDeleteResponse = await _client.SendAsync(deleteRequestMessage);

            List<ErrorDetail>? deleteCommandResult = await orderDeleteResponse.Content.ReadFromJsonAsync<List<ErrorDetail>?>();

            Assert.NotNull(deleteCommandResult);
            Assert.Equal(HttpStatusCode.NotFound, orderDeleteResponse.StatusCode);
            Assert.Contains(deleteCommandResult!, e => e.Code == "NOT_FOUND");
        }


        [Fact]
        public async Task EditOrder_ShouldReturnUpdatedOrder_WhenCommandValid()
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

            EditOrderCommand editCommand = new EditOrderCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                OrderId = orderCommandResult!.OrderId,
                OrderCompletionDate = DateTime.UtcNow.AddDays(14),
                OrderAddress = new OrderAddress
                {
                    Country = "Russia",
                    City = "Saint-Petersburg",
                    Region = "Leningrad",
                    Street = "Nevsky",
                }
            };

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/Orders/EditOrder/{orderCommandResult!.OrderId}");
            httpRequestMessage.Headers.Add("idempotencyKey", editCommand.IdempotencyKey);
            httpRequestMessage.Content = JsonContent.Create(editCommand);
            HttpResponseMessage editResponse = await _client.SendAsync(httpRequestMessage);
            string editResponseContent = await editResponse.Content.ReadAsStringAsync();
            ShowOrderDto? editCommandResult = await editResponse.Content.ReadFromJsonAsync<ShowOrderDto>();

            Assert.NotNull(editCommandResult);
            Assert.Equal(HttpStatusCode.OK, editResponse.StatusCode);
            Assert.Equal(editCommand.OrderId, editCommandResult!.OrderId);
            Assert.Equal(editCommand.OrderCompletionDate, editCommandResult.OrderCompletionDate);
            Assert.Equal(editCommand.OrderAddress.City, editCommandResult.OrderAddress.City);
        }


        [Fact]
        public async Task EditOrder_ShouldReturnNotFound_WhenOrderNotFound()
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

            EditOrderCommand editCommand = new EditOrderCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                OrderId = Guid.NewGuid(),
                OrderCompletionDate = DateTime.UtcNow.AddDays(7),
                OrderAddress = new OrderAddress
                {
                    Country = "Russia",
                    City = "Saint-Petersburg",
                    Region = "Leningrad",
                    Street = "Nevsky",
                }
            };

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/Orders/EditOrder/{Guid.NewGuid()}");
            httpRequestMessage.Headers.Add("idempotencyKey", editCommand.IdempotencyKey);
            httpRequestMessage.Content = JsonContent.Create(editCommand);
            HttpResponseMessage editResponse = await _client.SendAsync(httpRequestMessage);
            //editResponse.EnsureSuccessStatusCode();
            string editResponseContent = await editResponse.Content.ReadAsStringAsync();
            List<ErrorDetail>? editCommandResult = await editResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();

            Assert.NotNull(editCommandResult);
            Assert.Equal(HttpStatusCode.NotFound, editResponse.StatusCode);
            Assert.Contains(editCommandResult!, e => e.Code == "NOT_FOUND");
        }


        [Fact]
        public async Task GetOrderById_ShouldReturnOrder_WhenOrderExists()
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

            HttpResponseMessage getResponse = await _client.GetAsync($"/api/Orders/GetOrderById/{orderCommandResult!.OrderId}");
            getResponse.EnsureSuccessStatusCode();
            ShowOrderDto? getCommandResult = await getResponse.Content.ReadFromJsonAsync<ShowOrderDto>();

            Assert.NotNull(getCommandResult);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.Equal(orderCommandResult.OrderId, getCommandResult!.OrderId);
            Assert.Equal(orderCommandResult.BuyerId, getCommandResult.BuyerId);
            Assert.Equal(orderCommandResult.EmployeeId, getCommandResult.EmployeeId);
        }


        [Fact]
        public async Task GetOrderById_ShouldNotFound_WhenOrderNotExists()
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

            HttpResponseMessage getResponse = await _client.GetAsync($"/api/Orders/GetOrderById/{Guid.NewGuid()}");
            List<ErrorDetail>? getCommandResult = await getResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();

            Assert.NotNull(getCommandResult);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
            Assert.Contains(getCommandResult!, e => e.Code == "NOT_FOUND");
        }


        [Fact]
        public async Task GetOrderAll_ShouldReturnOrder_WhenOrdersExists()
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

            CreateBuyerCommand buyerCommand1 = new CreateBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerName = "Ignat",
                BuyerSurname = "Smirnov",
                BuyerMiddlename = "Ivanovich",
                BuyerEmail = "testEmail@example.com",
                PhoneNumber = "+79161234523",
                BuyerAddress = "Cherepovets, lubetskaya street"
            };
            HttpResponseMessage buyerPostResponse1 = await _client.PostAsJsonAsync("/api/Buyer/CreateBuyer", buyerCommand1);
            buyerPostResponse.EnsureSuccessStatusCode();
            ShowBuyerDto? buyerCommandResult1 = await buyerPostResponse1.Content.ReadFromJsonAsync<ShowBuyerDto>();

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
                CarManufacturer = "Lada",
                CarModelName = "Aura",
                CarConfiguration = new CarConfiguration
                {
                    CarInterior = "Leather",
                    WheelSize = 19.3F,
                    CarMusicBrand = "ural"
                },
                CarCountry = "Russia",
                ProductionDateTime = DateTime.UtcNow,
                CarEngine = new Engine
                {
                    FuelType = "Gasoline",
                    EngineCapacity = 2.5F,
                    EngineHorsePower = 203
                },
                CarPrice = 50000000,
                CarColor = "White",
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
                        OrderItemComments = "No comments"
                    }
                }
            };

            HttpResponseMessage orderPostResponse = await _client.PostAsJsonAsync("/api/Orders/CreateOrder", orderCommand);
            orderPostResponse.EnsureSuccessStatusCode();
            ShowOrderDto? orderCommandResult = await orderPostResponse.Content.ReadFromJsonAsync<ShowOrderDto>();

            CreateOrderCommand orderCommand1 = new CreateOrderCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerId = buyerCommandResult1!.BuyerId,
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
                        CarModelId = carModelCommandResult1!.CarModelId,
                        OrderQuantity = 2,
                        OrderItemComments = "No comments"
                    }
                }
            };

            HttpResponseMessage orderPostResponse1 = await _client.PostAsJsonAsync("/api/Orders/CreateOrder", orderCommand1);
            orderPostResponse1.EnsureSuccessStatusCode();
            ShowOrderDto? orderCommandResult1 = await orderPostResponse1.Content.ReadFromJsonAsync<ShowOrderDto>();

            HttpResponseMessage getResponse = await _client.GetAsync($"/api/Orders/GetAllOrders/All");
            getResponse.EnsureSuccessStatusCode();
            List<ShowOrderDto>? getCommandResult = await getResponse.Content.ReadFromJsonAsync<List<ShowOrderDto>>();
            
            Assert.NotNull(getCommandResult);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.True(getCommandResult!.Count >= 2);
        }
    }
}