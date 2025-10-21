using CreditTrack.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CreditTrack.Domain.IRepo
{
  public  interface IProductRepository
    {


        Task<int> AddProductAsync(Product product);

        Task<IEnumerable<Product>> GetAllAsync();

       Task<Product> UpdateProductAsync(int productId, Product product);

        Task<bool> SoftDeleteProductAsync(int productId);

        Task<Product> GetByIdAsync(int productId);
        Task<IEnumerable<Product>> GetAllWithCategoryAsync();
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    }
}
