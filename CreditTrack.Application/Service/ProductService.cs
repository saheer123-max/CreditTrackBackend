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
using AutoMapper;


namespace CreditTrack.Application.Service
{
    public class ProductService: IProductService
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
                    IsDeleted = false
                };

                int productId = await _repo.AddProductAsync(product);
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

        public async Task<ApiResponse<Product>> UpdateProductAsync(int productId, ProductDto dto)
        {
            try
            {
                var existingProduct = await _repo.GetByIdAsync(productId);
                if (existingProduct == null)
                    return ApiResponse<Product>.Fail("Product not found");

                existingProduct.Name = dto.Name;
                existingProduct.Price = dto.Price;
                existingProduct.CategoryId = dto.CategoryId;

                // ✅ Cloudinary upload logic
                if (dto.Image != null)
                {
                    string imageUrl = await _cloudinary.UploadImageAsync(dto.Image);
                    existingProduct.ImageUrl = imageUrl;
                }

                // ✅ രണ്ടും argument pass ചെയ്യണം ഇവിടെ
                var updatedProduct = await _repo.UpdateProductAsync(productId, existingProduct);

                if (updatedProduct == null)
                    return ApiResponse<Product>.Fail("Unable to update product");

                return ApiResponse<Product>.Ok(updatedProduct, "Product updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return ApiResponse<Product>.Fail("Internal server error");
            }
        }







        public async Task<ApiResponse<bool>> SoftDeleteProductAsync(int productId)
        {
            try
            {
                var deleted = await _repo.SoftDeleteProductAsync(productId);
                if (!deleted)
                {
                    _logger.LogWarning("Product with ID {Id} not found for deletion.", productId);
                    return ApiResponse<bool>.Fail("Product not found.");
                }

                _logger.LogInformation("Product with ID {Id} soft-deleted successfully.", productId);
                return ApiResponse<bool>.Ok(true, "Product deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SoftDeleteProductAsync");
                return ApiResponse<bool>.Fail("Internal server error.");
            }
        }


        public async Task<ApiResponse<Product>> GetByIdAsync(int productId)
        {
            try
            {
                var product = await _repo.GetByIdAsync(productId); // repository should check IsDeleted = 0
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {Id} not found.", productId);
                    return ApiResponse<Product>.Fail("Product not found.");
                }

                _logger.LogInformation("Product with ID {Id} retrieved successfully.", productId);
                return ApiResponse<Product>.Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdAsync for ID {Id}", productId);
                return ApiResponse<Product>.Fail("Internal server error.");
            }
        }



        public async Task<ApiResponse<IEnumerable<Product>>> GetAllProductsAsync()
        {
            try
            {
                var products = await _repo.GetAllWithCategoryAsync();
                return ApiResponse<IEnumerable<Product>>.Ok(products, "All products fetched");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all products");
                return ApiResponse<IEnumerable<Product>>.Fail("Failed to fetch products");
            }
        }

        public async Task<ApiResponse<IEnumerable<Product>>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                var products = await _repo.GetByCategoryAsync(categoryId);
                return ApiResponse<IEnumerable<Product>>.Ok(products, "Products fetched by category");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products by category");
                return ApiResponse<IEnumerable<Product>>.Fail("Failed to fetch products by category");
            }
        }


    }




}
