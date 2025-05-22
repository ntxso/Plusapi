using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace API.Models
{
    public class User
    {
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Username { get; set; }

        [Column(TypeName = "nvarchar(256)")]
        public string PasswordHash { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public string Role { get; set; } // Admin, Bayi, Editor

        public bool IsActive { get; set; } = true;

        // Foreign Key: Bağlı olduğu bayi (Customer)
        public int? CustomerId { get; set; } // Admin bağımsız olabilir
        public Customer? Customer { get; set; }
    }


}
