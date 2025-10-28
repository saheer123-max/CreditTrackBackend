using CreditTrack.Domain.Common;
using CreditTrack.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreditTrack.Application.DTOs;

namespace CreditTrack.Application.Interfaces
{
    public interface IProductService
    {

        Task<ApiResponse<int>> AddProductAsync(ProductDto productDto);

        // ➤ Product update ചെയ്യാൻ (FormData / DTO)
        Task<ApiResponse<Product>> UpdateProductAsync(int productId, ProductDto dto);

        // ➤ Product soft delete ചെയ്യാൻ
        Task<ApiResponse<bool>> SoftDeleteProductAsync(int productId);

        // ➤ Product ID അനുസരിച്ച് കൊണ്ടുവരാൻ
        Task<ApiResponse<Product>> GetByIdAsync(int productId);

        // ➤ എല്ലാ products-ഉം കൊണ്ടുവരാൻ
        Task<ApiResponse<IEnumerable<Product>>> GetAllProductsAsync();

        // ➤ Category അനുസരിച്ച് products കൊണ്ടുവരാൻ
        Task<ApiResponse<IEnumerable<Product>>> GetProductsByCategoryAsync(int categoryId);

        // ➤ എല്ലാ products (deleted, non-deleted എല്ലാം) കൊണ്ടുവരാൻ
        Task<IEnumerable<Product>> GetAllAsync();
    }
}
