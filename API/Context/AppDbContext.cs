using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CustomerProductPrice> CustomerProductPrices { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product - Stock (1-1)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Stock)
                .WithOne(s => s.Product)
                .HasForeignKey<Stock>(s => s.ProductId);

            // Product - Tag (1-1)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Tag)
                .WithOne(t => t.Product)
                .HasForeignKey<Tag>(t => t.ProductId);

            // Product - ProductImage (1-N)
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Images)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId);

            // Category - Product (1-N)
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId);

            // Customer - Order (1-N)
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Customer)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CustomerId)
                .OnDelete(DeleteBehavior.Restrict); // Adminler müşteriyle bağlı değilse silinmesin

            // Order - OrderItem (1-N)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId);

            // Product - CustomerProductPrice (1-N)
            modelBuilder.Entity<Product>()
                .HasMany(p => p.SpecialPrices)
                .WithOne(cp => cp.Product)
                .HasForeignKey(cp => cp.ProductId);

            // Customer - CustomerProductPrice (1-N)
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.SpecialPrices)
                .WithOne(cp => cp.Customer)
                .HasForeignKey(cp => cp.CustomerId);

            // Default values
            modelBuilder.Entity<Stock>()
               .Property(s => s.Quantity)
               .HasDefaultValue(0);

            // Decimal hassasiyet ayarları
            modelBuilder.Entity<Customer>()
                .Property(c => c.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CustomerProductPrice>()
                .Property(p => p.SpecialPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.BuyingPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);
        }

    }
}
