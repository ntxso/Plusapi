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

        public DbSet<PhoneModel> PhoneModels { get; set; }
        public DbSet<ProductPhoneModel> ProductPhoneModels { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Color> Colors { get; set; }
        public DbSet<ProductColor> ProductColors { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductColor>()
    .HasKey(pc => new { pc.ProductId, pc.ColorId });

            modelBuilder.Entity<ProductColor>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.ProductColors)
                .HasForeignKey(pc => pc.ProductId);

            modelBuilder.Entity<ProductColor>()
                .HasOne(pc => pc.Color)
                .WithMany(c => c.ProductColors)
                .HasForeignKey(pc => pc.ColorId);

            modelBuilder.Entity<OrderItem>()
    .HasOne(oi => oi.Color)
    .WithMany()
    .HasForeignKey(oi => oi.ColorId)
    .OnDelete(DeleteBehavior.Restrict); // opsiyonel

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.PhoneModel)
                .WithMany()
                .HasForeignKey(oi => oi.PhoneModelId)
                .OnDelete(DeleteBehavior.Restrict); // opsiyonel

            modelBuilder.Entity<Cart>()
    .HasMany(c => c.Items)
    .WithOne(i => i.Cart)
    .HasForeignKey(i => i.CartId);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Customer)
                .WithMany(cu => cu.Carts)
                .HasForeignKey(c => c.CustomerId);

            // CartItem opsiyonel Color ve PhoneModel ilişkileri
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Color)
                .WithMany()
                .HasForeignKey(ci => ci.ColorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.PhoneModel)
                .WithMany()
                .HasForeignKey(ci => ci.PhoneModelId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<ProductPhoneModel>()
        .HasKey(pp => new { pp.ProductId, pp.PhoneModelId });

            modelBuilder.Entity<ProductPhoneModel>()
                .HasOne(pp => pp.Product)
                .WithMany(p => p.CompatiblePhoneModels)
                .HasForeignKey(pp => pp.ProductId);

            modelBuilder.Entity<ProductPhoneModel>()
                .HasOne(pp => pp.PhoneModel)
                .WithMany(pm => pm.ProductPhoneModels)
                .HasForeignKey(pp => pp.PhoneModelId);

            // Product - Stock (1-1)
            //modelBuilder.Entity<Product>()
            //    .HasOne(p => p.Stock)
            //    .WithOne(s => s.Product)
            //    .HasForeignKey<Stock>(s => s.ProductId);

            // Stock - Product (N-1)
            modelBuilder.Entity<Stock>()
                .HasOne(s => s.Product)
                .WithMany(p => p.Stocks) // Product sınıfında ICollection<Stock> Stocks eklemelisiniz
                .HasForeignKey(s => s.ProductId);

            // Stock - PhoneModel (N-1, optional)
            modelBuilder.Entity<Stock>()
                .HasOne(s => s.PhoneModel)
                .WithMany()
                .HasForeignKey(s => s.PhoneModelId)
                .OnDelete(DeleteBehavior.Restrict); // Varyasyon silinirse stok kaydı silinmesin

            // Stock - Color (N-1, optional)
            modelBuilder.Entity<Stock>()
                .HasOne(s => s.Color)
                .WithMany()
                .HasForeignKey(s => s.ColorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Composite index (isteğe bağlı)
            modelBuilder.Entity<Stock>()
                .HasIndex(s => new { s.ProductId, s.PhoneModelId, s.ColorId })
                .IsUnique(false); // Aynı kombinasyonla birden fazla stok girişi olabilir

            modelBuilder.Entity<Stock>()
    .Property(s => s.LastUpdated)
    .HasDefaultValueSql("GETUTCDATE()");




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
