namespace API.Models
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? Barcode { get; set; }
        public int? CategoryId { get; set; }
        public decimal? Price { get; set; }
        public int? Publish { get; set; }
    }

}
