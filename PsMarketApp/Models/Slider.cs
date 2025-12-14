using System.ComponentModel.DataAnnotations;

namespace PsMarketApp.Models
{
    public class Slider
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Baslik { get; set; } // Örn: "Çılgın İndirimler"

        // Bir Slider'ın içinde birden çok ürün olabilir
        public List<Product>? Products { get; set; }
    }
}