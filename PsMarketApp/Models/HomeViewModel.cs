namespace PsMarketApp.Models
{
    public class HomeViewModel
    {
        // Kayan Kutular Listesi
        public List<Slider> Sliders { get; set; }

        // Tüm Ürünler Listesi (Slider'da olsun olmasın hepsi)
        public List<Product> Products { get; set; }
    }
}