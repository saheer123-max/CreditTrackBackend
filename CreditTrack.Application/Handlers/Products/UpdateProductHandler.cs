using CreditTrack.Application.Commands.Products;
using CreditTrack.Application.Commands.Products;
using CreditTrack.Application.DTOs;
using CreditTrack.Application.Interfaces;
using CreditTrack.Application.IRepo;
using CreditTrack.Domain.Model;
using MediatR;

namespace CreditTrack.Application.Handlers.Products
{
    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Product>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public UpdateProductHandler(IProductRepository productRepository, ICloudinaryService cloudinaryService)
        {
            _productRepository = productRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<Product> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
  
            var existingProduct = await _productRepository.GetByIdAsync(request.ProductId);
            if (existingProduct == null)
                return null;

    
            string imageUrl = existingProduct.ImageUrl;
            if (request.ProductDto.Image != null)
            {
                imageUrl = await _cloudinaryService.UploadImageAsync(request.ProductDto.Image);
            }

   
            existingProduct.Name = request.ProductDto.Name;
            existingProduct.Price = request.ProductDto.Price;
            existingProduct.CategoryId = request.ProductDto.CategoryId;
            existingProduct.ImageUrl = imageUrl;

            return await _productRepository.UpdateProductAsync(request.ProductId, existingProduct);
        }
    }
}
