using Azure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{

    public class Productx
    {
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Code { get; set; }

        [Column(TypeName = "nvarchar(255)")]
        public string Name { get; set; }

        

        public string Description { get; set; }
        public int Publish { get; set; }
        public decimal Price { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; }

        public Stock Stock { get; set; }
        public Tag Tag { get; set; }
        public ICollection<ProductImage> Images { get; set; }
    }


}
