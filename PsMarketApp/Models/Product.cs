using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PsMarketApp.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; } // Benzersiz Numara

        [Required]
        public string Name { get; set; } // Ürün Adı

        public string? Description { get; set; } // AÇIKLAMA (Eksikti, eklendi)

        public string ImageUrl { get; set; } // Resim Linki 

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Fiyat

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OldPrice { get; set; } // Eski Fiyat

        public string? Category { get; set; } // KATEGORİ (Eksikti, eklendi)

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // OLUŞTURULMA TARİHİ (Eksikti)

        public DateTime? DiscountEndDate { get; set; } // İNDİRİM BİTİŞ TARİHİ (Eksikti, eklendi)

        public bool Vitrin { get; set; } // Vitrinde gösterilsin mi?

        // --- İLİŞKİLER ---
        public int? SliderId { get; set; }

        [ForeignKey("SliderId")]
        public virtual Slider? Slider { get; set; }
    }
}