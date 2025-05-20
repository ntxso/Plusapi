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
        public string Code { get; set; }

        public string Description { get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public string? Barcode { get; set; }
        public int CategoryId { get; set; }
        public decimal? Price { get; set; }

        public int Publish { get; set; }
        public Category Category { get; set; }

        public Stock Stock { get; set; }
        public Tag Tag { get; set; }
        public ICollection<ProductImage> Images { get; set; }
    }

    public class Stock
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; }

        public int Quantity { get; set; }
    }

    public class Tag
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; }

        public string Value { get; set; }
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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Product? Product { get; set; }
    }

}
