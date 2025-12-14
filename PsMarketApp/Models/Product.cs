using System.ComponentModel.DataAnnotations;

namespace PsMarketApp.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; } // Her ürünün benzersiz numarası

        public string Name { get; set; } // Ürün Adı (Örn: GTA 5)

        public string ImageUrl { get; set; } // Resim Yolu

        public decimal Price { get; set; } // Fiyatı

        public decimal? OldPrice { get; set; } // Eski Fiyat
    }
}