using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;
using Respawn;
using Respawn.Graph;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using CarsTradeAPI.Data;


namespace CarsTradeTests.IntegrationTests.TestData
{
    /// <summary>
    /// Фикстура для управления жизненным циклом PostgreSQL контейнера и сброса данных между тестами
    /// </summary>
    public class PostgreSqlContainerFixture : IAsyncLifetime
    {
        public PostgreSqlContainer Container { get; private set; } = default!;
        public string ConnectionString { get; private set; } = string.Empty;
        private Respawner _respawner = default!;

        public async Task InitializeAsync()
        {
            // 1️ Создание PostgreSQL контейнера
            Container = new PostgreSqlBuilder()
                .WithImage("postgres:16")
                .WithDatabase($"cars_trade_test_{Guid.NewGuid():N}")
                .WithUsername("test_user")
                .WithPassword("test_password")
                .WithCleanUp(true)
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilCommandIsCompleted("pg_isready -U test_user"))
                .Build();

            await Container.StartAsync();
            ConnectionString = Container.GetConnectionString();

            // 2️ Применение миграции — создание таблиц
            var options = new DbContextOptionsBuilder<CarsTradeDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            await using (var db = new CarsTradeDbContext(options))
            {
                await db.Database.MigrateAsync();
            }

            // 3️ Создание Respawner
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();

            _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" },
                TablesToIgnore = new Table[] { new("__EFMigrationsHistory") },
            });
        }

        public async Task ResetDataAsync()
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();
            await _respawner.ResetAsync(conn);
        }

        public async Task DisposeAsync()
        {
            await Container.StopAsync();
            await Container.DisposeAsync();
        }
    }
}