using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Models
{
    public class Categoryx
    {
        public int Id { get; set; }

        [Column(TypeName = "char(50)")]
        public string Name { get; set; }

        [JsonIgnore] // Döngüye girilmemesi için
        public ICollection<Product> Products { get; set; }
    }
    //public class SubCategory
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }

    //    public int CategoryId { get; set; }
    //    public Category Category { get; set; }

    //    public ICollection<Product> Products { get; set; }
    //}

}
