using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace API.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }

        // İlişki
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }

    public class EmailVerification
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string Email { get; set; }
        [Column(TypeName = "nvarchar(6)")]
        public string Code { get; set; } // 6 haneli doğrulama kodu
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsUsed { get; set; }
        
    }
    public class User
    {
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Email { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public string PasswordHash { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public string Role { get; set; } // Admin, Dealer, Editor

        public bool IsActive { get; set; } = true;

        public bool? EmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }


        // Foreign Key: Bağlı olduğu bayi (Customer)
        public int? CustomerId { get; set; } // Admin bağımsız olabilir
        public Customer? Customer { get; set; }
    }


    public class DealerRegisterDto
    {
        // Customer (Bayi)
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Phone { get; set; }
        public int CityId { get; set; }
        public int DistrictId { get; set; }
        public string Address { get; set; }
        public SalesType SalesType { get; set; }
        public string? VerificationCode { get; set; }

        // User (Kullanıcı)
        public string Email { get; set; }
        public string Password { get; set; } // plain password
    }


}
