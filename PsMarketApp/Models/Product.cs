using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PsMarketApp.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; } // Bu sütun sende eksik olabilir!

        public string ImageUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OldPrice { get; set; }

        public string? Category { get; set; } // Bu da eksik olabilir

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? DiscountEndDate { get; set; } // Bu karışıklık yaratıyor olabilir

        public bool Vitrin { get; set; } // Vitrin ayarı

        // İlişkiler
        public int? SliderId { get; set; }
        [ForeignKey("SliderId")]
        public virtual Slider? Slider { get; set; }
    }
}