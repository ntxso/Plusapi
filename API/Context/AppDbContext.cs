using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<Category> Categories => Set<Category>();
       
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Stock> Stocks => Set<Stock>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Category - Product ilişkisi
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId);

            // Varsayılan değerler ve kolon tanımları
            //modelBuilder.Entity<Category>()
            //    .Property(c => c.Name)
            //    .HasColumnType("char(50)");

           

            modelBuilder.Entity<Product>()
                .Property(p => p.Publish)
                .HasDefaultValue(0);

            modelBuilder.Entity<Stock>()
                .Property(s => s.Quantity)
                .HasDefaultValue(1);

            

            // İlişkiler
            modelBuilder.Entity<Tag>()
                .HasOne(t => t.Product)
                .WithOne(p => p.Tag)
                .HasForeignKey<Tag>(t => t.ProductId);

            modelBuilder.Entity<Stock>()
                .HasOne(s => s.Product)
                .WithOne(p => p.Stock)
                .HasForeignKey<Stock>(s => s.ProductId);

            modelBuilder.Entity<ProductImage>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProductId);
        }

    }


}
