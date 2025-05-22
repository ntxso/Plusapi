using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace API.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }

        [MaxLength(50)]
        public string Role { get; set; } // "Admin", "Operator", "CustomerRep", vs.

        //public int? CompanyId { get; set; }
        //public Company Company { get; set; }

        public bool IsActive { get; set; } = true;
    }

}
