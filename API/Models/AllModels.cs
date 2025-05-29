using CloudinaryDotNet.Actions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace API.Models
{
    

    public class Category
    {
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Name { get; set; }

        [JsonIgnore] // Döngüye girilmemesi için
        public ICollection<Product> Products { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string? Code { get; set; }

        public string? Description { get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public string? Barcode { get; set; }
        public int? CategoryId { get; set; }
        public decimal? Price { get; set; }
        public decimal? BuyingPrice { get; set; }

        public int? Publish { get; set; }
        public Category? Category { get; set; }

        public Stock? Stock { get; set; }
        public Tag? Tag { get; set; }
        public ICollection<ProductImage>? Images { get; set; }
        public ICollection<CustomerProductPrice>? SpecialPrices { get; set; }

    }

    public class Stock
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        public int Quantity { get; set; }
    }

    public class Tag
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product? Product { get; set; }

        public string? Value { get; set; }
    }

    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public int? FileSizeKb { get; set; }

        public string PublicId { get; set; } // Yeni eklendi

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public Product? Product { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Title { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public string Phone { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        public string Address { get; set; }

        public decimal Balance { get; set; }

        public string? Notes { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string? TaxOffice { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public string? TaxValue { get; set; }

        // Yeni: Bayiye bağlı kullanıcılar
        public ICollection<User>? Users { get; set; }

        public ICollection<Order>? Orders { get; set; }
        public ICollection<CustomerProductPrice>? SpecialPrices { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTime? OrderDate { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem>? Items { get; set; }

        public decimal? TotalAmount { get; set; }
    }
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; } // O anki fiyat burada kayıt altına alınır

        public decimal TotalPrice => Quantity * UnitPrice;
    }
    public class CustomerProductPrice
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public decimal? SpecialPrice { get; set; }

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }



    class temp
    {
        private void ornekfiyatcek()
        {
            // Pseudo-code mantığı
            //var specialPrice = db.CustomerProductPrices
            //    .FirstOrDefault(x => x.CustomerId == customerId && x.ProductId == productId);

            //decimal unitPrice = specialPrice?.SpecialPrice ?? product.Price;

        }
    }

}
