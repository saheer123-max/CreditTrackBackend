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

    
        Task<ApiResponse<Product>> UpdateProductAsync(int productId, ProductDto dto);

   
        Task<ApiResponse<bool>> SoftDeleteProductAsync(int productId);

  
        Task<ApiResponse<Product>> GetByIdAsync(int productId);

    
        Task<ApiResponse<IEnumerable<Product>>> GetAllProductsAsync();

  
        Task<ApiResponse<IEnumerable<Product>>> GetProductsByCategoryAsync(int categoryId);


        Task<IEnumerable<Product>> GetAllAsync();
    }
}
