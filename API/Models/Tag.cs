namespace API.Models
{
    public class Tagx
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Tags { get; set; } = string.Empty;

        public Product? Product { get; set; }
    }

}
