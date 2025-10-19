namespace CarsTradeTests.IntegrationTests.TestData
{
    /// <summary>
    /// Базовый класс для интеграционных тестов
    /// </summary>
    [Collection("IntegrationTests")]
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        protected readonly PostgreSqlContainerFixture DbFixture;
        protected readonly CustomWebApplicationFactoryFixture FactoryFixture;
        protected readonly HttpClient _client;

        protected IntegrationTestBase(
            PostgreSqlContainerFixture dbFixture,
            CustomWebApplicationFactoryFixture factoryFixture)
        {
            DbFixture = dbFixture;
            FactoryFixture = factoryFixture;

            // Важно: гарантия, что фабрика инициализирована корректной строкой
            if (FactoryFixture.Factory == null)
                FactoryFixture.SetConnectionString(DbFixture.ConnectionString);

            _client = FactoryFixture.Factory.CreateClient();
        }

        public async Task InitializeAsync()
        {
            await DbFixture.ResetDataAsync();
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}