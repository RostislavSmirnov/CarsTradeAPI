using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


namespace CarsTradeAPI.Data
{
    /// <summary>
    /// Фабрика для создания экземпляров CarsTradeDbContext во время миграций
    /// </summary>
    public class CarsTradeDbContextFactory : IDesignTimeDbContextFactory<CarsTradeDbContext>
    {
        public CarsTradeDbContext CreateDbContext(string[] args)
        {
            // Строит IConfiguration, чтобы считать appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // указывает рабочую директорию
                .AddJsonFile("appsettings.json")              // подключает appsettings.json
                .Build();

            // Берёт строку подключения
            string? connectionString = configuration.GetConnectionString("DefaultConnection");

            // Создаёт DbContextOptions с этой строкой
            DbContextOptionsBuilder<CarsTradeDbContext> optionsBuilder = new DbContextOptionsBuilder<CarsTradeDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new CarsTradeDbContext(optionsBuilder.Options);
        }
    }
}

