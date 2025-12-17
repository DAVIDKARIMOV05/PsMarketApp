using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;

namespace PsMarketApp.Helpers
{
    public class ImageUploader
    {
        private readonly Account _account;
        private readonly Cloudinary _cloudinary;

        public ImageUploader()
        {
            // BURAYI KENDİ BİLGİLERİNLE DOLDUR
            _account = new Account(
                "dwipokmrn",
                "297892197845526",
                "R-aTZvqOavUBAjF6HfX49QXPP7M"
            );
            _cloudinary = new Cloudinary(_account);
        }

        public string UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.FileName, stream),
                        // İstersen resmi kare kırpabilirsin (Opsiyonel, şimdilik kapalı kalsın)
                        // Transformation = new Transformation().Width(500).Height(500).Crop("fill") 
                    };

                    var uploadResult = _cloudinary.Upload(uploadParams);
                    return uploadResult.SecureUrl.ToString(); // Bize https://... linkini verir
                }
            }
            catch (Exception)
            {
                return null; // Hata olursa null döner
            }
        }
    }
}