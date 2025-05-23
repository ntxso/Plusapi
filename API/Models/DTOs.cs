using System.ComponentModel.DataAnnotations.Schema;

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

    public class CreateCustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public decimal Balance { get; set; }
        public string Notes { get; set; }
    }
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }
    }
}
