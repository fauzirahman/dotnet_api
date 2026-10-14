using Microsoft.EntityFrameworkCore;
using Market.Models;


namespace Market.Persistence.Contexts
{
    public class AppDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<MarketUser> Users { get; set; }
        public DbSet<MarketOrder> Orders { get; set; }
        public DbSet<MarketOrderItem> OrderItems { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<Category>().ToTable("Categories");
            builder.Entity<Category>().HasKey(p => p.Id);
            builder.Entity<Category>().Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<Category>().Property(p => p.Name).IsRequired().HasMaxLength(30);
            builder.Entity<Category>().HasMany(p => p.Products).WithOne(p => p.Category).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Category>().HasData
            (
                new Category { Id = 100, Name = "Fruits and Vegetables" }, // Id set manually due to in-memory provider
                new Category { Id = 101, Name = "Dairy" }
            );

            builder.Entity<Product>().ToTable("Products");
            builder.Entity<Product>().HasKey(p => p.Id);
            builder.Entity<Product>().Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Entity<Product>().Property(p => p.Name).IsRequired().HasMaxLength(50);
            builder.Entity<Product>().Property(p => p.QuantityInPackage).IsRequired();
            builder.Entity<Product>().Property(p => p.UnitOfMeasurement).IsRequired();
            builder.Entity<Product>().Property(p => p.Price).HasPrecision(12, 2).IsRequired();
            builder.Entity<Product>().Property(p => p.StockQuantity).IsConcurrencyToken().IsRequired();

            builder.Entity<Product>().HasData
            (
                new Product
                {
                    Id = 100,
                    Name = "Apple",
                    QuantityInPackage = 1,
                    UnitOfMeasurement = EUnitOfMeasurement.Unity,
                    Price = 25000m,
                    StockQuantity = 100,
                    CategoryId = 100
                },
                new Product
                {
                    Id = 101,
                    Name = "Milk",
                    QuantityInPackage = 2,
                    UnitOfMeasurement = EUnitOfMeasurement.Liter,
                    Price = 35000m,
                    StockQuantity = 50,
                    CategoryId = 101,
                }
            );

            builder.Entity<MarketUser>().ToTable("Users");
            builder.Entity<MarketUser>().HasKey(user => user.Id);
            builder.Entity<MarketUser>().Property(user => user.Email).IsRequired().HasMaxLength(254);
            builder.Entity<MarketUser>().HasIndex(user => user.Email).IsUnique();
            builder.Entity<MarketUser>().Property(user => user.PasswordHash).IsRequired();
            builder.Entity<MarketUser>().Property(user => user.Role).IsRequired().HasMaxLength(20);

            builder.Entity<MarketOrder>().ToTable("Orders");
            builder.Entity<MarketOrder>().HasKey(order => order.Id);
            builder.Entity<MarketOrder>().Property(order => order.OrderNumber).IsRequired().HasMaxLength(40);
            builder.Entity<MarketOrder>().HasIndex(order => order.OrderNumber).IsUnique();
            builder.Entity<MarketOrder>().Property(order => order.Total).HasPrecision(12, 2).IsRequired();
            builder.Entity<MarketOrder>().Property(order => order.Status).IsRequired().HasMaxLength(20).IsConcurrencyToken();
            builder.Entity<MarketOrder>().Property(order => order.PaymentMethod).IsRequired().HasMaxLength(20);
            builder.Entity<MarketOrder>().Property(order => order.PaymentStatus).IsRequired().HasMaxLength(20);
            builder.Entity<MarketOrder>().HasOne(order => order.Customer).WithMany(user => user.Orders).HasForeignKey(order => order.CustomerId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<MarketOrder>().HasMany(order => order.Items).WithOne(item => item.Order).HasForeignKey(item => item.OrderId).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MarketOrderItem>().ToTable("OrderItems");
            builder.Entity<MarketOrderItem>().HasKey(item => item.Id);
            builder.Entity<MarketOrderItem>().Property(item => item.ProductName).IsRequired().HasMaxLength(50);
            builder.Entity<MarketOrderItem>().Property(item => item.UnitPrice).HasPrecision(12, 2).IsRequired();
            builder.Entity<MarketOrderItem>().HasOne(item => item.Product).WithMany().HasForeignKey(item => item.ProductId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}