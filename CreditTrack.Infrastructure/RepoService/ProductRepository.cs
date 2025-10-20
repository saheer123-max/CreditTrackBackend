using CreditTrack.Application.DTOs;
using CreditTrack.Domain.IRepo;
using CreditTrack.Domain.Model;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditTrack.Infrastructure.RepoService
{
   public class ProductRepository:IProductRepository
    {


        private readonly IDbConnection _db;

        public ProductRepository(IDbConnection db)
        {
            _db = db;
        }

       public async Task<int>  AddProductAsync(Product product)
        {
             string sql= @"INSERT INTO Products (Name, Price, CategoryId, ImageUrl,   IsDeleted)
             VALUES (@Name, @Price, @CategoryId, @ImageUrl,@IsDeleted); SELECT CAST(SCOPE_IDENTITY() as int)";

            return await _db.QuerySingleAsync<int>(sql, product);

        }


        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            string sql = @"SELECT * FROM Products";
            return await _db.QueryAsync<Product>(sql);
        }


        //public async Task<Product> UpdateProductAsync(int productId, Product product)
        //{
        //    // 1. Check if product exists
        //    string selectSql = @"SELECT * FROM Products WHERE Id = @Id AND IsDeleted = 0";
        //    var existingProduct = await _db.QueryFirstOrDefaultAsync<Product>(selectSql, new { Id = productId });

        //    if (existingProduct == null)
        //        return null; // Service layer can handle exception

        //    // 2. Update product
        //    string updateSql = @"
        //UPDATE Products
        //SET Name = @Name,
        //    Price = @Price,
        //    ImageUrl = @ImageUrl,
        //    CategoryId = @CategoryId
        //WHERE Id = @Id";

        //    await _db.ExecuteAsync(updateSql, new
        //    {
        //        Id = productId,
        //        Name = product.Name,
        //        Price = product.Price,
        //        ImageUrl = product.ImageUrl,
        //        CategoryId = product.CategoryId
        //    });

        //    // 3. Return updated entity
        //    var updatedProduct = await _db.QueryFirstOrDefaultAsync<Product>(selectSql, new { Id = productId });
        //    return updatedProduct;
        //}


    }
}
