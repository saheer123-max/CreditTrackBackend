using CreditTrack.Application.Commands.Products;
using CreditTrack.Application.Interfaces;
using CreditTrack.Application.IRepo;
using CreditTrack.Domain.Model;
using MediatR;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _repo;
    private readonly ICloudinaryService _cloudinary;

    public CreateProductHandler(IProductRepository repo, ICloudinaryService cloudinary)
    {
        _repo = repo;
        _cloudinary = cloudinary;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var dto = request.ProductDto;

        string imageUrl = null;
        if (dto.Image != null)
            imageUrl = await _cloudinary.UploadImageAsync(dto.Image);

        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            ImageUrl = imageUrl,
            IsDeleted = false
        };

        return await _repo.AddProductAsync(product);
    }
}

