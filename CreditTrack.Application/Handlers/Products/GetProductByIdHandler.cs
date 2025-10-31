using CreditTrack.Application.Queries.Products;
using CreditTrack.Application.IRepo;
using CreditTrack.Domain.Model;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CreditTrack.Application.Handlers.Products
{
    public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Product>
    {
        private readonly IProductRepository _productRepository;

        public GetProductByIdHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Product> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            return await _productRepository.GetByIdAsync(request.ProductId);
        }
    }
}
