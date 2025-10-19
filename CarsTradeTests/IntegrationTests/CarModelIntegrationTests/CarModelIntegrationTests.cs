using CarsTradeTests.IntegrationTests.TestData;
using System.Net.Http.Json;
using System.Net;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeAPI.Features.CarModelOperation.CreateCarModel;
using CarsTradeAPI.Entities.Elements;
using CarsTradeAPI.Features.CarModelOperation.CarModelDto;
using CarsTradeAPI.Features.CarModelOperation.EditCarModel;
using CarsTradeAPI.Features.CarModelOperation.GetCarModel;


namespace CarsTradeTests.IntegrationTests.CarModelIntegrationTests
{
    /// <summary>
    /// Класс интеграционных тестов для операций с моделью автомобиля
    /// </summary>
    public class CarModelIntegrationTests : IntegrationTestBase
    {
        public CarModelIntegrationTests(PostgreSqlContainerFixture dbFixture, CustomWebApplicationFactoryFixture factoryFixture)
            : base(dbFixture, factoryFixture) { }

        [Fact]
        public async Task CreateCarModel_ShouldReturnCarModel()
        {
            CreateCarModelCommand command = new CreateCarModelCommand
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

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", command);

            if (!postResponse.IsSuccessStatusCode)
            {
                var errorContent = await postResponse.Content.ReadAsStringAsync();
                Assert.Fail($"Запрос не успешен: {postResponse.StatusCode}, {errorContent}");
            }

            string responseContent = await postResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Ответ сервера: {responseContent}");

            ShowCarModelDto? created = await postResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            Assert.NotNull(created);
            Assert.Equal(command.CarModelName, created!.CarModelName);
            Assert.Equal(command.CarEngine.FuelType, created!.CarEngine!.FuelType);
        }


        [Fact]
        public async Task CreateCarModel_ShouldReturnBadRequest_WhenCommandInvalid() 
        {
            CreateCarModelCommand command = new CreateCarModelCommand
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
                CarCountry = "",
                ProductionDateTime = DateTime.UtcNow,
                CarEngine = new Engine
                {
                    FuelType = "Gasoline",
                    EngineCapacity = 2.5F,
                    EngineHorsePower = 203
                },
                CarPrice = 3000000,
                CarColor = "",
            };

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", command);
            List<ErrorDetail>? errors = await postResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();
            Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
            Assert.NotNull(postResponse.Content);
            Assert.NotNull(errors);
            Assert.Contains(errors, e => e.Code == "VALIDATION_ERROR");
        }


        [Fact]
        public async Task DeleteCarModel_ShouldReturnOk_WhenCommandValid() 
        {
            CreateCarModelCommand command = new CreateCarModelCommand
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

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", command);
            postResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? created = await postResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            HttpRequestMessage deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/CarModel/DeleteCarModel/{created!.CarModelId}");
            deleteRequest.Headers.Add("idempotencyKey", Guid.NewGuid().ToString());
            HttpResponseMessage deleteResponse = await _client.SendAsync(deleteRequest);

            deleteResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? deleted = await deleteResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();
            
            Assert.NotNull(deleted);
            Assert.Equal(created!.CarModelId, deleted!.CarModelId);
            Assert.Equal(created!.CarModelName, deleted!.CarModelName);
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }


        [Fact]
        public async Task DeleteCarModel_ShouldReturnBadRequest_WhenCarModelNotExist()
        {
            Guid nonExistentCarModelId = Guid.NewGuid();
            string idempotencyKey = Guid.NewGuid().ToString();
            HttpRequestMessage deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/CarModel/DeleteCarModel/{nonExistentCarModelId}");
            deleteRequest.Headers.Add("idempotencyKey", idempotencyKey);
            HttpResponseMessage deleteResponse = await _client.SendAsync(deleteRequest);
            List<ErrorDetail>? errors = await deleteResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>(); 
            
            Assert.NotNull(errors);
            Assert.Contains(errors, e => e.Code == "NOT_FOUND");
        }


        [Fact]
        public async Task EditCarModel_ShouldReturnOk_WhenCommandValid() 
        {
            CreateCarModelCommand command = new CreateCarModelCommand
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
            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", command);
            postResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? created = await postResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();
            EditCarModelCommand editCommand = new EditCarModelCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                CarModelId = created!.CarModelId,
                CarManufacturer = "Toyota",
                CarModelName = "Camry Edited",
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
                CarPrice = 3500000,
                CarColor = "Black",
            };
            HttpRequestMessage putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/CarModel/EditCarModel/{created!.CarModelId}");
            putRequest.Headers.Add("idempotencyKey", editCommand.IdempotencyKey);
            putRequest.Content = JsonContent.Create(editCommand);
            HttpResponseMessage putResponse = await _client.SendAsync(putRequest);

            putResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? edited = await putResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();
            Assert.NotNull(edited);
            Assert.Equal(editCommand.CarModelId, edited!.CarModelId);
            Assert.Equal(editCommand.CarModelName, edited!.CarModelName);
            Assert.Equal(editCommand.CarColor, edited!.CarColor);
            Assert.Equal(editCommand.CarPrice, edited!.CarPrice);
        }


        [Fact]
        public async Task EditCarModelCommand_ShouldReturnbadRequest_WhenCarModelInvalid() 
        {
            CreateCarModelCommand command = new CreateCarModelCommand
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
            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", command);
            postResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? created = await postResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();
            EditCarModelCommand editCommand = new EditCarModelCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                CarModelId = created!.CarModelId,
                CarCountry = "Japan",
                ProductionDateTime = DateTime.UtcNow,
                CarEngine = new Engine
                {
                    FuelType = "Water",
                    EngineCapacity = 2.5F,
                    EngineHorsePower = 203
                },
                CarPrice = 3500000,
                CarColor = "Black",
            };
            HttpRequestMessage putRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/CarModel/EditCarModel/{created!.CarModelId}");
            putRequest.Headers.Add("idempotencyKey", editCommand.IdempotencyKey);
            putRequest.Content = JsonContent.Create(editCommand);
            HttpResponseMessage putResponse = await _client.SendAsync(putRequest);

            List<ErrorDetail>? editedError = await putResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();
            Assert.NotNull(editedError);
            Assert.Contains(editedError, e => e.Code == "VALIDATION_ERROR");
        }


        [Fact]
        public async Task GetAllCarModel_ReturnOk() 
        {
            CreateCarModelCommand command1 = new CreateCarModelCommand
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
            HttpResponseMessage postResponse1 = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", command1);
            postResponse1.EnsureSuccessStatusCode();

            CreateCarModelCommand command2 = new CreateCarModelCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                CarManufacturer = "Toyota",
                CarModelName = "Camry 3.5",
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
                    EngineCapacity = 3.5F,
                    EngineHorsePower = 310
                },
                CarPrice = 3000000,
                CarColor = "White",
            };
            HttpResponseMessage postResponse2 = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", command2);
            postResponse2.EnsureSuccessStatusCode();


            GetCarModelQuery query = new GetCarModelQuery();
            HttpResponseMessage getResponse = await _client.GetAsync("/api/CarModel/GetCarModels/All");
            Assert.NotNull(getResponse);
            getResponse.EnsureSuccessStatusCode();
            List<ShowCarModelDto>? list = await getResponse.Content.ReadFromJsonAsync<List<ShowCarModelDto>>();
            Assert.NotNull(list);
            Assert.True(list!.Count >= 2);
        }


        [Fact]
        public async Task GetCarModelById_ReturnOk() 
        {
            CreateCarModelCommand command = new CreateCarModelCommand
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
            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/CarModel/CreateCarModel", command);
            postResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? created = await postResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();

            HttpResponseMessage getResponse = await _client.GetAsync($"/api/CarModel/GetCarModel/{created!.CarModelId}");
            getResponse.EnsureSuccessStatusCode();
            ShowCarModelDto? getById = await getResponse.Content.ReadFromJsonAsync<ShowCarModelDto>();
            Assert.NotNull(getById);
            Assert.Equal(created!.CarModelId, getById!.CarModelId);
            Assert.Equal(created!.CarModelName, getById!.CarModelName);
        }


        [Fact]
        public async Task GetCarModelById_ReturnbadRequest_WhenCarModelNotExist() 
        {
            Guid testId = Guid.NewGuid();
            HttpRequestMessage deleteRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/CarModel/GetCarModel/{testId}");
            HttpResponseMessage deleteResponse = await _client.SendAsync(deleteRequest);
            List<ErrorDetail>? errors = await deleteResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();
            Assert.NotNull(errors);
            Assert.Contains(errors, e => e.Code == "NOT_FOUND");
        }
    }
}
