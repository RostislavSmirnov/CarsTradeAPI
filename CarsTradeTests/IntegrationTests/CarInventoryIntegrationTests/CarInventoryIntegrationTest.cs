using System.Net.Http.Json;
using CarsTradeAPI.Entities.Elements;
using CarsTradeAPI.Features.CarInventoryOperation.CreateCarInventory;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Features.CarModelOperation.CreateCarModel;
using CarsTradeTests.IntegrationTests.TestData;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryDto;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using System.Net;
using Microsoft.AspNetCore.Http;
using CarsTradeAPI.Features.CarInventoryOperation.EditCarInventory;


namespace CarsTradeTests.IntegrationTests.CarInventoryIntegrationTests
{
    /// <summary>
    /// Класс интеграционных тестов для операций с инвентаризацией автомобилей
    /// </summary>
    [Collection("IntegrationTests")]
    public class CarInventoryIntegrationTest : IntegrationTestBase
    {
        public CarInventoryIntegrationTest(PostgreSqlContainerFixture dbFixture, CustomWebApplicationFactoryFixture factoryFixture)
            : base(dbFixture, factoryFixture) { }

        [Fact]
        public async Task CreateCarInventory_ReturnOk_WhenCommandValid()
        {
            CreateCarModelCommand carCommand = new CreateCarModelCommand
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

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carCommand);
            postResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? createdCarModel = await postResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarInventoryCommand inventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = createdCarModel!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", inventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();
            Assert.NotNull(createdInventory);
            Assert.Equal(inventoryCommand.Quantity, (int)createdInventory!.Quantity);
            Assert.Equal(inventoryCommand.CarModelId, createdInventory.CarModelId);
        }


        [Fact]
        public async Task CreateCarInventory_ReturnOk_WhenCommandInvalid()
        {
            CreateCarModelCommand carCommand = new CreateCarModelCommand
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

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carCommand);
            postResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? createdCarModel = await postResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarInventoryCommand inventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = createdCarModel!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = -1
            };

            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", inventoryCommand);
            List<ErrorDetail>? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();
            Assert.NotNull(createdInventory);
            Assert.Equal(HttpStatusCode.BadRequest, inventoryResponse.StatusCode);
            Assert.Contains(createdInventory!, e => e.Code == "VALIDATION_ERROR");
        }


        [Fact]
        public async Task DeleteCarInventory_ShouldReturnOk_WhenCommandValid()
        {
            CreateCarModelCommand carCommand = new CreateCarModelCommand
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

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carCommand);
            postResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? createdCarModel = await postResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarInventoryCommand inventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = createdCarModel!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", inventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            HttpRequestMessage deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/CarInventory/DeleteCarInventory/{createdInventory!.InventoryId}");
            deleteRequest.Headers.Add("idempotencyKey", Guid.NewGuid().ToString());
            HttpResponseMessage deleteResponse = await _client.SendAsync(deleteRequest);
            deleteResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? deletedInventory = await deleteResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();
            Assert.NotNull(deletedInventory);
            Assert.Equal(createdInventory.InventoryId, deletedInventory!.InventoryId);
            Assert.Equal(createdInventory.CarModelId, deletedInventory.CarModelId);
        }


        [Fact]
        public async Task DeleteCarInventory_ShouldReturnOk_WhenCommandInvalid()
        {
            CreateCarModelCommand carCommand = new CreateCarModelCommand
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

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carCommand);
            postResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? createdCarModel = await postResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarInventoryCommand inventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = createdCarModel!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", inventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            HttpRequestMessage deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/CarInventory/DeleteCarInventory/{Guid.NewGuid()}");
            deleteRequest.Headers.Add("idempotencyKey", Guid.NewGuid().ToString());
            HttpResponseMessage deleteResponse = await _client.SendAsync(deleteRequest);
            List<ErrorDetail>? deletedInventoryErrors = await deleteResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();
            Assert.NotNull(deletedInventoryErrors);
            Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            Assert.Contains(deletedInventoryErrors, e => e.Code == "NOT_FOUND");
        }


        [Fact]
        public async Task EditCarInventory_ShouldReturnOk_WhenCommandValid()
        {
            CreateCarModelCommand carCommand = new CreateCarModelCommand
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

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carCommand);
            postResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? createdCarModel = await postResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarInventoryCommand inventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = createdCarModel!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", inventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            EditCarInventoryCommand editCommand = new EditCarInventoryCommand
            {
                InventoryId = createdInventory!.InventoryId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 5
            };
            HttpRequestMessage editRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/CarInventory/EditCarInventory/{createdInventory!.InventoryId}");
            editRequest.Headers.Add("idempotencyKey", Guid.NewGuid().ToString());
            editRequest.Content = JsonContent.Create(editCommand);
            HttpResponseMessage editResponse = await _client.SendAsync(editRequest);
            editResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? editedInventory = await editResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            Assert.NotNull(editedInventory);
            Assert.Equal(editCommand.InventoryId, editedInventory!.InventoryId);
            Assert.Equal(editCommand.Quantity, (int)editedInventory.Quantity);
        }


        [Fact]
        public async Task EditCarInventory_ShouldReturnBadRequest_WhenCommandInvalid()
        {
            CreateCarModelCommand carCommand = new CreateCarModelCommand
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

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carCommand);
            postResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? createdCarModel = await postResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarInventoryCommand inventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = createdCarModel!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", inventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            EditCarInventoryCommand editCommand = new EditCarInventoryCommand
            {
                InventoryId = createdInventory!.InventoryId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = -1
            };
            HttpRequestMessage editRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/CarInventory/EditCarInventory/{createdInventory!.InventoryId}");
            editRequest.Headers.Add("idempotencyKey", Guid.NewGuid().ToString());
            editRequest.Content = JsonContent.Create(editCommand);
            HttpResponseMessage editResponse = await _client.SendAsync(editRequest);

            List<ErrorDetail>? editedInventoryErrors = await editResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();

            Assert.NotNull(editedInventoryErrors);
            Assert.Equal(HttpStatusCode.BadRequest, editResponse.StatusCode);
            Assert.Contains(editedInventoryErrors, e => e.Code == "VALIDATION_ERROR");
        }


        [Fact]
        public async Task GetCarinventoryById_ReturnOk_WhenCarinventoryExists()
        {
            CreateCarModelCommand carCommand = new CreateCarModelCommand
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

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carCommand);
            postResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? createdCarModel = await postResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarInventoryCommand inventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = createdCarModel!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", inventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            HttpResponseMessage getCarInventoryMessage = await _client.GetAsync($"api/Carinventory/GetCarInventory/{createdInventory!.InventoryId}");
            getCarInventoryMessage.EnsureSuccessStatusCode();
            ShowCarInventoryDto? getResponse = await getCarInventoryMessage.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            Assert.Equal(HttpStatusCode.OK, getCarInventoryMessage.StatusCode);
            Assert.NotNull(getResponse);
            Assert.Equal(getResponse.CarModelId, inventoryCommand.CarModelId);
            Assert.Equal((int)getResponse.Quantity, inventoryCommand.Quantity);
        }


        [Fact]
        public async Task GetCarinventoryById_ReturnNotFound_WhenCarinventoryNotExists()
        {
            CreateCarModelCommand carCommand = new CreateCarModelCommand
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

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carCommand);
            postResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? createdCarModel = await postResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            CreateCarInventoryCommand inventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = createdCarModel!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", inventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            HttpResponseMessage getCarInventoryMessage = await _client.GetAsync($"api/Carinventory/GetCarInventory/{Guid.NewGuid()}");
            List<ErrorDetail>? getResponse = await getCarInventoryMessage.Content.ReadFromJsonAsync<List<ErrorDetail>>();

            Assert.NotNull(getResponse);
            Assert.Equal(HttpStatusCode.NotFound, getCarInventoryMessage.StatusCode);
            Assert.Contains(getResponse, e => e.Code == "NOT_FOUND");
        }


        [Fact]
        public async Task GetAllCarInventories_ReturnOk_WhenCarInventoriesExists()
        {
            CreateCarModelCommand carCommand = new CreateCarModelCommand
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
            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", carCommand);
            postResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? createdCarModel = await postResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();
            CreateCarInventoryCommand inventoryCommand = new CreateCarInventoryCommand
            {
                CarModelId = createdCarModel!.CarModelId,
                IdempotencyKey = Guid.NewGuid().ToString(),
                Quantity = 10
            };
            HttpResponseMessage inventoryResponse = await _client.PostAsJsonAsync("/api/CarInventory/CreateCarInventory", inventoryCommand);
            inventoryResponse.EnsureSuccessStatusCode();
            ShowCarInventoryDto? createdInventory = await inventoryResponse.Content.ReadFromJsonAsync<ShowCarInventoryDto>();

            HttpResponseMessage getAllResponse = await _client.GetAsync("api/Carinventory/GetCarInventory/All");
            getAllResponse.EnsureSuccessStatusCode();
            List<ShowCarInventoryDto>? getAllInventories = await getAllResponse.Content.ReadFromJsonAsync<List<ShowCarInventoryDto>>();

            Assert.NotNull(getAllInventories);
            Assert.Contains(getAllInventories!, i => i.InventoryId == createdInventory!.InventoryId);
        }
    }
}
