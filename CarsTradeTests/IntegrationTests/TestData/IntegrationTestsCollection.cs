namespace CarsTradeTests.IntegrationTests.TestData
{
    /// <summary>
    /// Коллекция интеграционных тестов, связывающая необходимые фикстуры
    /// </summary>
    [CollectionDefinition("IntegrationTests")]
    public class IntegrationTestsCollection :
        ICollectionFixture<PostgreSqlContainerFixture>,
        ICollectionFixture<CustomWebApplicationFactoryFixture>
    {
        //связывает фикстуры с коллекцией
    }
}
