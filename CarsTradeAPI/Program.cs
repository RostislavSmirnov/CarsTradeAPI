
using CarsTradeAPI.Data;
using CarsTradeAPI.Entities;
using CarsTradeAPI.Features.BuyerOperation.BuyerRepository;
using CarsTradeAPI.Features.CarInventoryOperation.CarInventoryRepository;
using CarsTradeAPI.Features.CarModelOperation.CarModelRepository;
using CarsTradeAPI.Features.EmployeeOperation.EmployeeRepository;
using CarsTradeAPI.Features.OrdersOperation.OrdersRepository;
using CarsTradeAPI.Infrastructure.ErrorHandlerMiddleware;
using CarsTradeAPI.Infrastructure.Services.CacheService;
using CarsTradeAPI.Infrastructure.Services.CarEngineFuelService;
using CarsTradeAPI.Infrastructure.Services.CarInventoryService;
using CarsTradeAPI.Infrastructure.Services.GenerateTokenService;
using CarsTradeAPI.Infrastructure.Services.IdempotencyService;
using CarsTradeAPI.Infrastructure.ValidationBehavior;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Serilog;
using System;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;


namespace CarsTradeAPI
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            // Серилог конфигурация
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("CarsTradeApiLogs/CarsTradeApi_log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            try
            {
                // Создание билдер приложения
                var builder = WebApplication.CreateBuilder(args);


                // Добавление Serilog
                object value = builder.Host.UseSerilog();


                // Добавление NpgsqlDataSource с поддержкой JSONB
                builder.Services.AddSingleton(provider =>
                {
                    var dataSourceBuilder = new NpgsqlDataSourceBuilder(
                        builder.Configuration.GetConnectionString("DefaultConnection"));

                    dataSourceBuilder.EnableDynamicJson();

                    var dataSource = dataSourceBuilder.Build();
                    return dataSource;
                });


                // Добавление контекста базы данных с использованием NpgsqlDataSource
                builder.Services.AddDbContext<CarsTradeDbContext>((serviceProvider, options) =>
                {
                    var dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();
                    options.UseNpgsql(dataSource);
                });


                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

                // MediatR
                builder.Services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
                });


                // FluentValidation
                builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
                builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));


                // AutoMapper
                builder.Services.AddAutoMapper(typeof(Program));
                builder.Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    });


                // Redis
                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = builder.Configuration["Redis:ConnectionString"];
                    options.InstanceName = "CarsTradeAPI_";
                });


                // Проверка окружения для тестов
                string environment = builder.Environment.EnvironmentName;
                Console.WriteLine($"Environment: {environment}");

                if (!builder.Environment.IsEnvironment("Testing"))
                {
                    // MassTransit with RabbitMQ
                    string rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
                    string rabbitUser = builder.Configuration["RabbitMQ:User"] ?? "guest";
                    string rabbitPassword = builder.Configuration["RabbitMQ:Pass"] ?? "guest";

                    builder.Services.AddMassTransit(x =>
                    {
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host(rabbitMqHost, "/", h =>
                            {
                                h.Username(rabbitUser);
                                h.Password(rabbitPassword);
                            });
                        });
                    }); 
                }
                else
                {
                    Console.WriteLine("MassTransit отключён для тестов");
                }

                builder.Services.AddMassTransitHostedService();

                // Repositories
                builder.Services.AddScoped<ICarModelRepository, ImplementationCarModelRepository>();
                builder.Services.AddScoped<ICarInventoryRepository, ImplementationCarInventoryRepository>();
                builder.Services.AddScoped<IEmployeeRepository, ImplementationEmployeeRepository>();
                builder.Services.AddScoped<IBuyerRepository, ImplementationBuyerRepository>();
                builder.Services.AddScoped<IOrderRepository, ImplementationOrderRepository>();
                

                // Services 
                builder.Services.AddScoped<IGenerateToken, GenerateTokenService>();
                builder.Services.AddScoped<IPasswordHasher<Employee>, PasswordHasher<Employee>>();
                builder.Services.AddScoped<ICarInventoryService, CarInventoryService>();
                builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();
                builder.Services.AddScoped<ICacheService, RedisCacheService>();
                builder.Services.AddScoped<ICarEngineFuelService, CarEngineFuelService>();
                builder.Services.AddEndpointsApiExplorer();


                // Jwt Swagger
                builder.Services.AddSwaggerGen(options =>
                {
                    // Получение пути к XML-документации
                    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                    // Подключение XML-комментарии
                    options.IncludeXmlComments(xmlPath);

                    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                        Description = "Введите JWT токен в формате: Bearer {your token}"
                    });

                    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                    {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },

                        new string[] {} //Roles
                    },
                    });
                });


                // JWT authentication
                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                        ClockSkew = TimeSpan.Zero
                    };
                });


                var app = builder.Build();

                app.UseMiddleware<ErrorHandlerMiddleware>();

                // Применик миграций автоматически
                using (IServiceScope scope = app.Services.CreateScope())
                {
                    CarsTradeDbContext dbContext = scope.ServiceProvider.GetRequiredService<CarsTradeDbContext>();
                    dbContext.Database.MigrateAsync();
                }

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                app.UseAuthentication(); 
                app.UseAuthorization();

                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Приложение завершило работу с ошибкой");
                throw new Exception($"Ошибка: {ex}");
            }
        }
    }
}
