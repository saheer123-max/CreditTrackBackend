using CreditTrack.Application.Interfaces;
using CreditTrack.Domain.IRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CreditTrack.Domain.Common;
using CreditTrack.Application.DTOs;
using CreditTrack.Domain.Model;

namespace CreditTrack.Application.Service
{
   public class ProductService
    {

        private readonly IProductRepository _repo;

        private readonly ICloudinaryService _cloudinary;

        private readonly ILogger<ProductService> _logger;



        public ProductService(IProductRepository repo, ICloudinaryService cloudinary, ILogger<ProductService> logger)
        {
            _repo = repo;
            _cloudinary = cloudinary;
            _logger = logger;
        }

        public async Task<ApiResponse<int>> AddProductAsync(ProductDto productDto)
        {

            try
            {
               string imageUrl = string.Empty;

                if (productDto.Image != null)
                {
                    imageUrl = await _cloudinary.UploadImageAsync(productDto.Image);
                }

                var product = new Product
                {
                    Name = productDto.Name,
                    Price = productDto.Price,
                    CategoryId = productDto.CategoryId,
                    ImageUrl = imageUrl,
                    IsDeleted=false
                };

                int productId=    await _repo.AddProductAsync(product);
                return ApiResponse<int>.Ok(productId, "Product added successfully");
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error while adding product");
                return ApiResponse<int>.Fail("Something went wrong");


            }





        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {

            try
            {
                _logger.LogInformation("Fetching all products from database...");
                var products = await _repo.GetAllAsync();

                if (!products.Any())
                {
                    _logger.LogWarning("No products found in database.");
                }

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all products.");
                throw; // handled by GlobalExceptionMiddleware
            }

        }



    }
}
