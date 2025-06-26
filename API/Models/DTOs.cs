using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class TokenRefreshRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class CreateOrderDto
    {
        public int CustomerId { get; set; }
        public string Note { get; set; }
    }

    public class CartItemDto
    {
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int? ColorId { get; set; }
        public int? PhoneModelId { get; set; }
        public int Quantity { get; set; }
    }


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
        public string CompanyName { get; set; }
        public string Phone { get; set; }
        public int CityId { get; set; }
        public int DistrictId { get; set; }
        public string Address { get; set; }
        public SalesType SalesType { get; set; }

    }
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Name { get; set; }
    }

}
