using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class ProductImagex
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
