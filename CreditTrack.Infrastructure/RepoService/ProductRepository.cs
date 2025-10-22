using CreditTrack.Application.DTOs;
using CreditTrack.Domain.IRepo;
using CreditTrack.Domain.Model;
using Dapper;
using Microsoft.Data.SqlClient;
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


        public async Task<Product> UpdateProductAsync(int productId, Product product)
        {
            // 1. Check if product exists
            string checkSql = @"IF EXISTS (SELECT 1 FROM Products WHERE Id = @Id AND IsDeleted = 0)
                    SELECT 1
                    ELSE
                    SELECT 0";

int exists = await _db.ExecuteScalarAsync<int>(checkSql, new { Id = productId });

if (exists == 0)
    return null; // Service layer can handle exception

            // 2. Update product
            string updateSql = @"
        UPDATE Products
        SET Name = @Name,
            Price = @Price,
            ImageUrl = @ImageUrl,
            CategoryId = @CategoryId
        WHERE Id = @Id";

            await _db.ExecuteAsync(updateSql, new
            {
                Id = productId,
                Name = product.Name,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId
            });

            // 3. Return updated entity
            var updatedProduct = await _db.QueryFirstOrDefaultAsync<Product>(updateSql, new { Id = productId });
            return updatedProduct;
        }


        public async Task<bool> SoftDeleteProductAsync(int productId)
        {
            string sql = @"UPDATE Products 
                   SET IsDeleted = 1 
                   WHERE Id = @Id AND IsDeleted = 0";

            var rowsAffected = await _db.ExecuteAsync(sql, new { Id = productId });

            return rowsAffected > 0; // true if delete successful
        }



        public async Task<Product> GetByIdAsync(int productId)
        {
            string sql = @"SELECT * FROM Products WHERE Id = @Id AND IsDeleted = 0";
            return await _db.QueryFirstOrDefaultAsync<Product>(sql, new { Id = productId });
        }




        public async Task<IEnumerable<Product>> GetAllWithCategoryAsync()
        {
           
            var sql = @"SELECT p.*, c.Id, c.Name 
                    FROM Products p
                    INNER JOIN Category c ON p.CategoryId = c.Id
                      WHERE p.isDeleted = 0";
                        

            var dict = new Dictionary<int, Product>();

            var result = await _db.QueryAsync<Product, Category, Product>(
                sql,
                (product, category) =>
                {
                    if (!dict.TryGetValue(product.Id, out var current))
                    {
                        current = product;
                        current.Category = category;
                        dict.Add(current.Id, current);
                    }
                    return current;
                },
                splitOn: "Id"
            );

            return result.Distinct();
        }

        // Get products by category
        public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
        {
          
            var sql = @"SELECT p.*, c.Id, c.Name 
                    FROM Products p
                    INNER JOIN Category c ON p.CategoryId = c.Id
                    WHERE c.Id = @CategoryId";

            var dict = new Dictionary<int, Product>();

            var result = await _db.QueryAsync<Product, Category, Product>(
                sql,
                (product, category) =>
                {
                    if (!dict.TryGetValue(product.Id, out var current))
                    {
                        current = product;
                        current.Category = category;
                        dict.Add(current.Id, current);
                    }
                    return current;
                },
                new { CategoryId = categoryId },
                splitOn: "Id"
            );

            return result.Distinct();
        }

    }
}
