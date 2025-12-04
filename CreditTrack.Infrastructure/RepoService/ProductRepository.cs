    using CreditTrack.Application.DTOs;
    using CreditTrack.Application.IRepo;
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

            public async Task<int> AddProductAsync(Product product)
            {
                string sql = @"
            INSERT INTO products 
            (name, price, categoryid, imageurl, isdeleted)
            VALUES (@Name, @Price, @CategoryId, @ImageUrl, @IsDeleted)
            RETURNING id;
        ";

                return await _db.QuerySingleAsync<int>(sql, product);
            }



            public async Task<IEnumerable<Product>> GetAllAsync()
            {
            string sql = @"SELECT * FROM products WHERE isdeleted = false;";
            return await _db.QueryAsync<Product>(sql);
            }



            public async Task<Product> UpdateProductAsync(int productId, Product product)
            {
                // 1. Check product exists
                string checkSql = @"
            SELECT COUNT(*) 
            FROM products 
            WHERE id = @Id AND isdeleted = false;
        ";

                int exists = await _db.ExecuteScalarAsync<int>(checkSql, new { Id = productId });
                if (exists == 0)
                    return null;

                // 2. Update product
                string updateSql = @"
            UPDATE products
            SET name = @Name,
                price = @Price,
                imageurl = @ImageUrl,
                categoryid = @CategoryId
            WHERE id = @Id;
        ";

                await _db.ExecuteAsync(updateSql, new
                {
                    Id = productId,
                    Name = product.Name,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    CategoryId = product.CategoryId
                });

                // 3. Fetch updated product
                string selectSql = @"
            SELECT * 
            FROM products 
            WHERE id = @Id AND isdeleted = false;
        ";

                var updatedProduct = await _db.QueryFirstOrDefaultAsync<Product>(selectSql, new { Id = productId });
                return updatedProduct;
            }



            public async Task<bool> SoftDeleteProductAsync(int productId)
            {
                string sql = @"
            UPDATE products
            SET isdeleted = true
            WHERE id = @Id AND isdeleted = false;
        ";

                var rowsAffected = await _db.ExecuteAsync(sql, new { Id = productId });

                return rowsAffected > 0;
            }




        public async Task<Product> GetByIdAsync(int productId)
        {
            string sql = @"
        SELECT *
        FROM products
        WHERE id = @Id AND isdeleted = false;
    ";

            return await _db.QueryFirstOrDefaultAsync<Product>(sql, new { Id = productId });
        }




        public async Task<IEnumerable<Product>> GetAllWithCategoryAsync()
            {
                var sql = @"
            SELECT 
                p.id AS productid,
                p.name AS productname,
                p.price,
                p.imageurl,
                p.categoryid,
                p.isdeleted,
                c.id AS categoryid_split,
                c.name AS categoryname
            FROM products p
            INNER JOIN category c ON p.categoryid = c.id
            WHERE p.isdeleted = false;
        ";

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
                    splitOn: "categoryid_split"
                );

                return result.Distinct();
            }


            public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
            {
                var sql = @"
            SELECT
                p.id AS productid,
                p.name AS productname,
                p.price,
                p.imageurl,
                p.categoryid,
                p.isdeleted,
                c.id AS categoryid_split,
                c.name AS categoryname
            FROM products p
            INNER JOIN category c ON p.categoryid = c.id
            WHERE c.id = @CategoryId
              AND p.isdeleted = false;
        ";

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
                    splitOn: "categoryid_split"
                );

                return result.Distinct();
            }


        }
    }
