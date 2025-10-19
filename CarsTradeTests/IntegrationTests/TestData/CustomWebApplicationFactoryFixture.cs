namespace CarsTradeTests.IntegrationTests.TestData
{
    /// <summary>
    /// Фикстура для CustomWebApplicationFactory
    /// </summary>
    public class CustomWebApplicationFactoryFixture : IAsyncLifetime
    {
        public CustomWebApplicationFactory Factory { get; private set; } = default!;
        public string ConnectionString { get; private set; } = string.Empty;

        public Task InitializeAsync() => Task.CompletedTask;

        public void SetConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
            Factory = new CustomWebApplicationFactory(connectionString);
        }

        public Task DisposeAsync()
        {
            Factory?.Dispose();
            return Task.CompletedTask;
        }
    }
}
