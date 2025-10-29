using CreditTrack.Application.Commands.Products;
using CreditTrack.Domain.IRepo;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace CreditTrack.Application.Handlers.Products
{
    public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IProductRepository _productRepository;

        public DeleteProductHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            return await _productRepository.SoftDeleteProductAsync(request.ProductId);
        }
    }
}
