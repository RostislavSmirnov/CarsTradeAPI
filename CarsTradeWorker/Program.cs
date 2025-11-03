using CarsTradeWorker.Consumers;
using MassTransit;
using Serilog;


namespace CarsTradeWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Настройка Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            var builder = Host.CreateApplicationBuilder(args);

            // Подключаем Serilog к системе логирования .NET
            builder.Logging.ClearProviders(); // убираем стандартный логгер
            builder.Logging.AddSerilog();     // добавляем Serilog

            // Настройка MassTransit
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderCreatedConsumer>();
                x.AddConsumer<OrderDeletedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("carstradeapi_rabbitmq", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ReceiveEndpoint("order-created-queue", e =>
                    {
                        e.ConfigureConsumer<OrderCreatedConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("order-deleted-queue", e =>
                    {
                        e.ConfigureConsumer<OrderDeletedConsumer>(context);
                    });
                });
            });

            builder.Services.AddMassTransitHostedService(true);
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}