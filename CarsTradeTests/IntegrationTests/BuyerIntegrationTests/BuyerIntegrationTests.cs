using System.Net;
using System.Net.Http.Json;
using CarsTradeAPI.Features.BuyerOperation.BuyerDto;
using CarsTradeAPI.Features.BuyerOperation.CreateBuyer;
using CarsTradeAPI.Features.BuyerOperation.EditBuyer;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using CarsTradeTests.IntegrationTests.TestData;


namespace CarsTradeTests.IntegrationTests.BuyerIntegrationTests
{
    /// <summary>
    /// Класс интеграционных тестов для операций с покупателями
    /// </summary>
    [Collection("IntegrationTests")]
    public class BuyerIntegrationTests : IntegrationTestBase
    {
        public BuyerIntegrationTests(PostgreSqlContainerFixture dbFixture, CustomWebApplicationFactoryFixture factoryFixture)
            : base(dbFixture, factoryFixture) { }

        [Fact]
        public async Task CreateBuyer_Then_GetAllBuyers_Works()
        {
            CreateBuyerCommand command = new CreateBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerName = "Ivan",
                BuyerSurname = "Petrov",
                BuyerMiddlename = "Sergeevich",
                BuyerEmail = "ivan.petrov@example.com",
                PhoneNumber = "+79161234567",
                BuyerAddress = "Moscow, Red Square"
            };

            HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/Buyer/CreateBuyer", command);
            postResponse.EnsureSuccessStatusCode();

            ShowBuyerDto? created = await postResponse.Content.ReadFromJsonAsync<ShowBuyerDto>();
            Assert.NotNull(created);
            Assert.Equal(command.BuyerEmail, created!.BuyerEmail);

            HttpResponseMessage getResponse = await     _client.GetAsync("/api/Buyer/GetAllBuyers");
            getResponse.EnsureSuccessStatusCode();

            List<ShowBuyerDto>? list = await getResponse.Content.ReadFromJsonAsync<List<ShowBuyerDto>>();
            Assert.NotNull(list);
            Assert.Contains(list!, b => b.BuyerEmail == command.BuyerEmail);
        }

        [Fact]
        public async Task CreateBuyer_WithInvalidEmail_ReturnsBadRequest()
        {
            CreateBuyerCommand command = new CreateBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerName = "Ivan",
                BuyerSurname = "Petrov",
                BuyerMiddlename = "Sergeevich",
                BuyerEmail = "invalid-email",
                PhoneNumber = "+79161234567",
                BuyerAddress = "Moscow, Red Square"
            };

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Buyer/CreateBuyer", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            List<ErrorDetail>? errors = await response.Content.ReadFromJsonAsync<List<ErrorDetail>>();
            Assert.NotNull(errors);
            Assert.Contains(errors, e => e.Code == "VALIDATION_ERROR");
            Assert.Contains(errors, e => e.Message.Contains("email", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task DeleteBuyer_AndGetAllBuyer_Works()
        {
            CreateBuyerCommand buyer1 = new CreateBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerName = "Alice",
                BuyerSurname = "Smith",
                BuyerMiddlename = "Johnson",
                BuyerEmail = "alice@gmail.com",
                PhoneNumber = "89215677898",
                BuyerAddress = "Los Angeles"
            };

            CreateBuyerCommand buyer2 = new CreateBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerName = "John",
                BuyerSurname = "Smith",
                BuyerMiddlename = "Johnson",
                BuyerEmail = "john@gmail.com",
                PhoneNumber = "89215677888",
                BuyerAddress = "Los Angeles"
            };

            HttpResponseMessage resp1 = await _client.PostAsJsonAsync("/api/Buyer/CreateBuyer", buyer1);
            HttpResponseMessage resp2 = await _client.PostAsJsonAsync("/api/Buyer/CreateBuyer", buyer2);
            resp1.EnsureSuccessStatusCode();
            resp2.EnsureSuccessStatusCode();

            ShowBuyerDto? created1 = await resp1.Content.ReadFromJsonAsync<ShowBuyerDto>();
            ShowBuyerDto? created2 = await resp2.Content.ReadFromJsonAsync<ShowBuyerDto>();
            Assert.NotNull(created1);
            Assert.NotNull(created2);

            string idempotencyKey = Guid.NewGuid().ToString();
            HttpRequestMessage deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/Buyer/DeleteBuyer/{created1!.BuyerId}");
            deleteRequest.Headers.Add("X-Idempotency-Key", idempotencyKey);

            HttpResponseMessage deleteResponse = await _client.SendAsync(deleteRequest);
            deleteResponse.EnsureSuccessStatusCode();

            ShowBuyerDto? deleted = await deleteResponse.Content.ReadFromJsonAsync<ShowBuyerDto>();
            Assert.NotNull(deleted);
            Assert.Equal("Alice", deleted!.BuyerName);

            HttpResponseMessage listResponse = await _client.GetAsync("/api/Buyer/GetAllBuyers");
            listResponse.EnsureSuccessStatusCode();

            List<ShowBuyerDto>? list = await listResponse.Content.ReadFromJsonAsync<List<ShowBuyerDto>>();
            Assert.NotNull(list);
            Assert.DoesNotContain(list!, b => b.BuyerId == created1.BuyerId);
        }

        [Fact]
        public async Task EditBuyer_WhenCommandValid()
        {
            CreateBuyerCommand createCommand = new CreateBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerName = "Alice",
                BuyerSurname = "Smith",
                BuyerMiddlename = "Johnson",
                BuyerEmail = "alice@gmail.com",
                PhoneNumber = "89655677811",
                BuyerAddress = "Los Angeles"
            };

            HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/Buyer/CreateBuyer", createCommand);
            createResponse.EnsureSuccessStatusCode();
            ShowBuyerDto? created = await createResponse.Content.ReadFromJsonAsync<ShowBuyerDto>();
            Assert.NotNull(created);

            EditBuyerCommand editCommand = new EditBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerId = created!.BuyerId,
                BuyerName = "EditedName",
                BuyerEmail = "edited@gmail.com",
                BuyerAddress = "EditedAddress"
            };

            HttpResponseMessage putResponse = await _client.PutAsJsonAsync("/api/Buyer/EditBuyer", editCommand);
            putResponse.EnsureSuccessStatusCode();

            ShowBuyerDto? edited = await putResponse.Content.ReadFromJsonAsync<ShowBuyerDto>();
            Assert.NotNull(edited);
            Assert.Equal("EditedName", edited!.BuyerName);
            Assert.Equal("edited@gmail.com", edited.BuyerEmail);
            Assert.Equal("EditedAddress", edited.BuyerAddress);
        }

        [Fact]
        public async Task EditBuyer_WhenCommandInvalid()
        {
            CreateBuyerCommand createCommand = new CreateBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerName = "John",
                BuyerSurname = "Smith",
                BuyerMiddlename = "Johnson",
                BuyerEmail = "john@gmail.com",
                PhoneNumber = "89345677813",
                BuyerAddress = "Los Angeles"
            };

            HttpResponseMessage resp = await _client.PostAsJsonAsync("/api/Buyer/CreateBuyer", createCommand);
            resp.EnsureSuccessStatusCode();
            ShowBuyerDto? created = await resp.Content.ReadFromJsonAsync<ShowBuyerDto>();
            Assert.NotNull(created);

            EditBuyerCommand invalidEdit = new EditBuyerCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                BuyerId = created!.BuyerId,
                BuyerName = "Edited",
                BuyerEmail = "InvalidEmail",
                BuyerAddress = "EditedAddress"
            };

            HttpResponseMessage putResponse = await _client.PutAsJsonAsync("/api/Buyer/EditBuyer", invalidEdit);

            Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
            List<ErrorDetail>? errors = await putResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();
            Assert.NotNull(errors);
            Assert.Contains(errors!, e => e.Code == "VALIDATION_ERROR");
            Assert.Contains(errors!, e => e.Message.Contains("Неверный формат email адреса"));
        }
    }
}
