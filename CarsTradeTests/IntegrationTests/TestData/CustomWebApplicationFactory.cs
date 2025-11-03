using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using CarsTradeAPI.Data;
using CarsTradeAPI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;


namespace CarsTradeTests.IntegrationTests.TestData
{
    /// <summary>
    /// Кастомная фабрика веб-приложения для интеграционных тестов
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;
        private static int _dbContextCount = 0;

        public CustomWebApplicationFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                Console.WriteLine("Настройка тестовых сервисов...");

                // 1️ Удаление оригинальной зависимости приложения
                services.RemoveAll<DbContextOptions<CarsTradeDbContext>>();
                services.RemoveAll<CarsTradeDbContext>();
                services.RemoveAll<IDistributedCache>();

                // 2️ Подмена кэша на InMemory
                services.AddSingleton<IDistributedCache, MemoryDistributedCache>();

                // 3️ Добавляем тестовую аутентификацию
                services
                    .AddAuthentication("TestAuth") // Используем тестовую схему
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestAuth", _ => { });

                // 4️ Подменяем авторизацию, чтобы всё разрешать
                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder("TestAuth")
                        .RequireAuthenticatedUser()
                        .Build();
                });

                // 5 Настройка NpgsqlDataSource
                services.AddSingleton<NpgsqlDataSource>(sp =>
                {
                    NpgsqlDataSourceBuilder dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
                    dataSourceBuilder.EnableDynamicJson(); // поддержка JSONB
                    NpgsqlDataSource dataSource = dataSourceBuilder.Build();
                    Console.WriteLine($"NpgsqlDataSource создан: {DateTime.UtcNow}");
                    return dataSource;
                });

                // 6 Настройка DbContext с NpgsqlDataSource
                services.AddScoped<CarsTradeDbContext>(sp =>
                {
                    NpgsqlDataSource dataSource = sp.GetRequiredService<NpgsqlDataSource>();

                    DbContextOptionsBuilder<CarsTradeDbContext> optionsBuilder = new DbContextOptionsBuilder<CarsTradeDbContext>()
                        .UseNpgsql(dataSource, npgsqlOptions =>
                        {
                            npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(3), null);
                        })
                        .EnableSensitiveDataLogging()
                        .EnableDetailedErrors();

                    CarsTradeDbContext dbContext = new CarsTradeDbContext(optionsBuilder.Options);
                    Console.WriteLine($"CarsTradeDbContext #{Interlocked.Increment(ref _dbContextCount)} создан в {DateTime.UtcNow}");
                    return dbContext;
                });

                services.AddSingleton<MassTransit.IPublishEndpoint, FakePublishEndpoint>();

                // 7 Применяем миграции при старте тестов
                ServiceProvider sp = services.BuildServiceProvider();
                using IServiceScope scope = sp.CreateScope();
                CarsTradeDbContext db = scope.ServiceProvider.GetRequiredService<CarsTradeDbContext>();
                db.Database.Migrate();

                Console.WriteLine("Тестовые сервисы успешно настроены.");
            });
        }
    }


    /// <summary>
    /// Кастомный обработчик аутентификации для тестов.
    /// Всегда "аутентифицирует" фиктивного пользователя без JWT.
    /// </summary>
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Создание подставного пользователя, будто он прошёл авторизацию
            Claim[] claims = new[]
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "Admin") // можно менять роль, если нужно
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            AuthenticationTicket ticket = new AuthenticationTicket(principal, "TestAuth");

            // Возвращенин успешного результата
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}