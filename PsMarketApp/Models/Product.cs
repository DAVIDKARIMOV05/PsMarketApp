using System.ComponentModel.DataAnnotations;

namespace PsMarketApp.Models
{
    public class Product
    {
        public bool Vitrin { get; set; }
        [Key]
        public int Id { get; set; } // Her ürünün benzersiz numarası

        public string Name { get; set; } // Ürün Adı (Örn: GTA 5)

        public string ImageUrl { get; set; } // Resim Yolu

        public decimal Price { get; set; } // Fiyatı

        public decimal? OldPrice { get; set; } // Eski Fiyat
                                               // Her ürünün bir Slider'ı olabilir (Boş da olabilir)
        public int? SliderId { get; set; }
        public Slider? Slider { get; set; }
    }
}