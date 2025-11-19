using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.NetworkInformation;
using System.Security.Principal;
using static System.Net.Mime.MediaTypeNames;
using CreditTrack.Application.Interfaces;

namespace CreditTrack.Application.Service
{
   public class CloudinaryService : ICloudinaryService
    {

        private readonly Cloudinary _cloudinary;
 
         public CloudinaryService(IConfiguration config)
        {
            var account = new Account
            (

                 config["Cloudinary:CloudName"],
                 config["Cloudinary:ApiKey"],
                 config["Cloudinary:ApiSecret"]



            );
               _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new Exception("No file provided for upload.");

                var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "products"
                };

                Console.WriteLine("Uploading image to Cloudinary..."); 

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result == null)
                    throw new Exception("Cloudinary result is null.");

                if (result.SecureUrl == null)
                    throw new Exception("Cloudinary upload failed: SecureUrl is null.");

                if (result.Error != null)
                    throw new Exception($"Cloudinary upload error: {result.Error.Message}");

                Console.WriteLine($"Upload successful! URL: {result.SecureUrl}"); 

                return result.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
               
                Console.WriteLine("=== Cloudinary Upload Debug Info ===");
                Console.WriteLine($"Exception Message: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                Console.WriteLine("==");

                throw; 
            }
        }



    }
}
