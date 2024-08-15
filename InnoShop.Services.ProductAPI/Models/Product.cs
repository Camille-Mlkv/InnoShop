using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InnoShop.Services.ProductAPI.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        [Range(1,1000)]
        public double Price { get; set; }
        public bool IsAvailable { get; set; }

        public string UserId { get; set; }

        public DateTime Date {  get; set; }

    }
}
