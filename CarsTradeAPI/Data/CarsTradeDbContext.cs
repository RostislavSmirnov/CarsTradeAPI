using Microsoft.EntityFrameworkCore;
using CarsTradeAPI.Entities;


namespace CarsTradeAPI.Data
{
    /// <summary>
    /// DbContext для CarsTradeAPI
    /// </summary>
    public class CarsTradeDbContext : DbContext
    {
        public CarsTradeDbContext(DbContextOptions<CarsTradeDbContext> options): base(options)
        {
            Console.WriteLine("CarsTradeDbContext initialized.");
        }
        
        public DbSet<CarModel> CarModels { get; set; }
        public DbSet<CarInventory> CarInventories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<IdempotencyRequest> IdempotencyRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Console.WriteLine("Configuring OnModelCreating...");

            // Настройка сущности CarModel
            modelBuilder.Entity<CarModel>()
                .ToTable("CarModels")
                .HasKey(cm => cm.CarModelId);

            modelBuilder.Entity<CarModel>()
                .HasMany(cm => cm.CarInventories)
                .WithOne(ci => ci.CarModel)
                .HasForeignKey(ci => ci.CarModelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CarModel>()
                .HasMany(cm => cm.OrderItems)
                .WithOne(oi => oi.CarModel)
                .HasForeignKey(oi => oi.CarModelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка сущности Order
            modelBuilder.Entity<Order>()
                .ToTable("Orders")
                .HasKey(o => o.OrderId);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Buyer)
                .WithMany(b => b.Orders)
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Employee)
                .WithMany(e => e.Orders)
                .HasForeignKey(o => o.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка сущности IdempotencyRequest
            modelBuilder.Entity<IdempotencyRequest>()
                .ToTable("IdempotencyRequests")
                .HasKey(ir => ir.Id);

            modelBuilder.Entity<IdempotencyRequest>()
                .Property(ir => ir.Key)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<IdempotencyRequest>()
                .Property(ir => ir.Status)
                .HasMaxLength(50);

            modelBuilder.Entity<IdempotencyRequest>()
                .Property(ir => ir.ResourceType)
                .HasMaxLength(50);

            // Настройка индексов
            modelBuilder.Entity<CarModel>()
                .HasIndex(cm => new { cm.CarManufacturer, cm.CarModelName })
                .HasDatabaseName("IX_CarModel_Manufacturer_ModelName");

            modelBuilder.Entity<CarInventory>()
                .HasIndex(ci => ci.CarModelId)
                .HasDatabaseName("IX_CarInventory_CarModelId");

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.EmployeeLogin)
                .IsUnique()
                .HasDatabaseName("IX_Employee_Login");

            // Настройка jsonb для PostgreSQL
            modelBuilder.Entity<CarModel>()
                .Property(cm => cm.CarConfiguration)
                .HasColumnType("jsonb");

            modelBuilder.Entity<CarModel>()
                .Property(cm => cm.CarEngine)
                .HasColumnType("jsonb");
                

            modelBuilder.Entity<Order>()
                .Property(o => o.OrderAddress)
                .HasColumnType("jsonb");

            Console.WriteLine("OnModelCreating configuration completed.");
        }
    }
}