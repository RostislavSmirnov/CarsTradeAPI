using CarsTradeAPI.Data;
using CarsTradeAPI.Features.EmployeeOperation.CreateEmployee;
using CarsTradeTests.IntegrationTests.TestData;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Xunit;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeDto;
using CarsTradeAPI.Features.EmployeeOperation.EditEmployee;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Xunit.Abstractions;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using System.Text.Json;


namespace CarsTradeTests.IntegrationTests.EmployeeTests
{
    [Collection("IntegrationTests")]
    public class CreateEmployeeTests : IntegrationTestBase
    {
        public CreateEmployeeTests(PostgreSqlContainerFixture dbFixture, CustomWebApplicationFactoryFixture factoryFixture)
            : base(dbFixture, factoryFixture) { }

        [Fact]
        public async Task CreateEmployee_ShouldReturnEmployee()
        {
            CreateEmployeeCommand command = new CreateEmployeeCommand
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

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Employee/CreateEmployee", command);
            response.EnsureSuccessStatusCode();

            ShowEmployeeDto? result = await response.Content.ReadFromJsonAsync<ShowEmployeeDto>();

            Assert.NotNull(result);
            Assert.Equal(command.EmployeeName, result!.EmployeeName);
            Assert.Equal(command.EmployeeSurname, result.EmployeeSurname);
        }


        [Fact]
        public async Task CreateEmployee_ShouldReturnBadRequest_WhenCommandInvalid() 
        {
            CreateEmployeeCommand command = new CreateEmployeeCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                EmployeeName = "",
                EmployeeSurname = "U",
                EmployeeMiddlename = "Integration",
                EmployeeAge = 30,
                EmployeeLogin = "test_login",
                EmployeePassword = "TestPass123!",
                EmployeeRole = "Admin"
            };
            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Employee/CreateEmployee", command);
            List<ErrorDetail>? errors = await response.Content.ReadFromJsonAsync<List<ErrorDetail>>();
            Assert.NotNull(errors);
            Assert.Contains(errors!, e => e.Code == "VALIDATION_ERROR");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Fact]
        public async Task DeleteEmployee_ShouldReturnOk_WhenEmployeeDeleted() 
        {
            CreateEmployeeCommand command = new CreateEmployeeCommand
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

            HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/Employee/CreateEmployee", command);
            createResponse.EnsureSuccessStatusCode();
            ShowEmployeeDto? createdResult = await createResponse.Content.ReadFromJsonAsync<ShowEmployeeDto>();
            
            HttpRequestMessage deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/Employee/DeleteEmployee/{createdResult!.EmployeeId}");
            deleteRequest.Headers.Add("idempotencyKey", Guid.NewGuid().ToString());
            HttpResponseMessage deleteResponse = await _client.SendAsync(deleteRequest);

            ShowEmployeeDto? deletedResult = await deleteResponse.Content.ReadFromJsonAsync<ShowEmployeeDto>();
            
            Assert.NotNull(deletedResult);
            Assert.Equal(createdResult.EmployeeId, deletedResult!.EmployeeId);
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }


        [Fact]
        public async Task DeleteEmployee_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
        {
            Guid nonExistentId = Guid.NewGuid();
            HttpRequestMessage deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/Employee/DeleteEmployee/{nonExistentId}");
            deleteRequest.Headers.Add("idempotencyKey", Guid.NewGuid().ToString());
            HttpResponseMessage deleteResponse = await _client.SendAsync(deleteRequest);
            List<ErrorDetail>? errors = await deleteResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();
            Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            Assert.NotNull(errors);
            Assert.Contains(errors!, e => e.Code == "NOT_FOUND");
        }


        [Fact]
        public async Task EditEmployee_ShouldReturnOk_WhenCommandValid() 
        {
            CreateEmployeeCommand createCommand = new CreateEmployeeCommand
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

            HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/Employee/CreateEmployee", createCommand);
            createResponse.EnsureSuccessStatusCode();
            ShowEmployeeDto? createdResult = await createResponse.Content.ReadFromJsonAsync<ShowEmployeeDto>();

            EditEmployeeCommand editCommand = new EditEmployeeCommand
            {
                IdempotencyKey = null,
                EmployeeId = null,
                EmployeeName = "EditedName",
                EmployeeSurname = "EditedSurname",
                EmployeeMiddlename = "EditedMiddlename",
                EmployeeAge = 35,
                EmployeeRole = "User"
            };

            HttpRequestMessage editRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/Employee/EditEmployee/{createdResult!.EmployeeId}");
            editRequest.Headers.Add("idempotencyKey", Guid.NewGuid().ToString());
            editRequest.Content = JsonContent.Create(editCommand);

            HttpResponseMessage editResponse = await _client.SendAsync(editRequest);
            string result = await editResponse.Content.ReadAsStringAsync();
            ShowEmployeeDto? editedResult = await editResponse.Content.ReadFromJsonAsync<ShowEmployeeDto>();

            Assert.NotNull(editedResult);
            Assert.Equal(editCommand.EmployeeName, editedResult!.EmployeeName);
            Assert.Equal(HttpStatusCode.OK, editResponse.StatusCode);
        }


        [Fact]
        public async Task EditEmployee_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
        {
            EditEmployeeCommand editCommand = new EditEmployeeCommand
            {
                IdempotencyKey = null,
                EmployeeId = null,
                EmployeeName = "EditedName",
                EmployeeSurname = "EditedSurname",
                EmployeeMiddlename = "EditedMiddlename",
                EmployeeAge = 35,
                EmployeeRole = "User"
            };

            Guid nonExistentId = Guid.NewGuid();
            HttpRequestMessage editRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/Employee/EditEmployee/{nonExistentId}");
            editRequest.Headers.Add("idempotencyKey", Guid.NewGuid().ToString());
            editRequest.Content = JsonContent.Create(editCommand);

            HttpResponseMessage editResponse = await _client.SendAsync(editRequest);
            string result = await editResponse.Content.ReadAsStringAsync();
            List<ErrorDetail>? errors = await editResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();

            Assert.Equal(HttpStatusCode.NotFound, editResponse.StatusCode);
            Assert.NotNull(errors);
            Assert.Contains(errors!, e => e.Code == "NOT_FOUND");
        }


        [Fact]
        public async Task GetAllEmployee_ShouldReturnAllEmployees()
        {
            CreateEmployeeCommand command1 = new CreateEmployeeCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                EmployeeName = "Test1",
                EmployeeSurname = "User1",
                EmployeeMiddlename = "Integration1",
                EmployeeAge = 30,
                EmployeeLogin = "test_login1",
                EmployeePassword = "TestPass123!",
                EmployeeRole = "Admin"
            };

            CreateEmployeeCommand command2 = new CreateEmployeeCommand
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                EmployeeName = "Test2",
                EmployeeSurname = "User2",
                EmployeeMiddlename = "Integration2",
                EmployeeAge = 28,
                EmployeeLogin = "test_login2",
                EmployeePassword = "TestPass123!",
                EmployeeRole = "User"
            };

            await _client.PostAsJsonAsync("/api/Employee/CreateEmployee", command1);
            await _client.PostAsJsonAsync("/api/Employee/CreateEmployee", command2);
            
            HttpResponseMessage response = await _client.GetAsync("/api/Employee/GetAllEmployee/All");
            response.EnsureSuccessStatusCode();

            List<ShowEmployeeDto>? employees = await response.Content.ReadFromJsonAsync<List<ShowEmployeeDto>>();
            Assert.NotNull(employees);
            Assert.True(employees!.Count >= 2);
            Assert.Contains(employees, e => e.EmployeeAge == command1.EmployeeAge);
            Assert.Contains(employees, e => e.EmployeeAge == command2.EmployeeAge);
        }


        [Fact]
        public async Task GetEmployeeById_ShouldReturnEmployee_WhenEmployeeExists()
        {
            CreateEmployeeCommand command = new CreateEmployeeCommand
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
            HttpResponseMessage createResponse = await _client.PostAsJsonAsync("/api/Employee/CreateEmployee", command);
            createResponse.EnsureSuccessStatusCode();
            ShowEmployeeDto? createdResult = await createResponse.Content.ReadFromJsonAsync<ShowEmployeeDto>();

            HttpResponseMessage getResponse = await _client.GetAsync($"/api/Employee/GetEmployeeById/{createdResult!.EmployeeId}");
            getResponse.EnsureSuccessStatusCode();
            ShowEmployeeDto? fetchedResult = await getResponse.Content.ReadFromJsonAsync<ShowEmployeeDto>();

            Assert.NotNull(fetchedResult);
            Assert.Equal(createdResult.EmployeeId, fetchedResult!.EmployeeId);
        }


        [Fact]
        public async Task GetEmployeeById_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
        {
            Guid nonExistentId = Guid.NewGuid();
            HttpResponseMessage getResponse = await _client.GetAsync($"/api/Employee/GetEmployeeById/{nonExistentId}");
            List<ErrorDetail>? errors = await getResponse.Content.ReadFromJsonAsync<List<ErrorDetail>>();
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
            Assert.NotNull(errors);
            Assert.Contains(errors!, e => e.Code == "NOT_FOUND");
        }
    }
}
