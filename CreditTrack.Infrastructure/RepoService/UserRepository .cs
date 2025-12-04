using CreditTrack.Application.IRepo;
using CreditTrack.Domain.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.Common;

namespace CreditTrack.Infrastructure.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _db;

        public UserRepository(IDbConnection db) => _db = db;

        public async Task<int> CreateUserAsync(User user)
        {
            string sql = @"INSERT INTO users 
                   (username, email, passwordhash, isactive, createdat, role)
                   VALUES (@Username, @Email, @PasswordHash, @IsActive, @CreatedAt, @Role)
                   RETURNING id;";

            return await _db.QuerySingleAsync<int>(sql, user);
        }


        public async Task<User> GetUserByEmailAsync(string email)
        {
            const string sql = @"SELECT id, username, email, passwordhash, isactive, createdat, role
                         FROM users
                         WHERE email = @Email";

            return await _db.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            const string sql = @"
        SELECT id, username, email, passwordhash, isactive, createdat, role
        FROM users
        WHERE username = @Username;
    ";

            return await _db.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            const string sql = @"
        SELECT id, username, email, passwordhash, isactive, createdat, role
        FROM users
        WHERE role <> 'Admin'
        ORDER BY createdat DESC;
    ";

            return await _db.QueryAsync<User>(sql);
        }




        public async Task<IEnumerable<User>> SearchUsersAsync(string keyword)
        {
            string query = @"
        SELECT id, username, email
        FROM users
        WHERE username ILIKE @Keyword;
    ";

            return await _db.QueryAsync<User>(query, new { Keyword = $"%{keyword}%" });
        }




        public async Task<(int totalCustomers, int totalSuppliers)> GetUserCountsAsync()
        {
            const string sql = @"
        SELECT
            SUM(CASE WHEN role = 'customer' THEN 1 ELSE 0 END) AS totalCustomers,
            SUM(CASE WHEN role = 'supplier' THEN 1 ELSE 0 END) AS totalSuppliers
        FROM users;
    ";

            var result = await _db.QueryFirstAsync<(int totalCustomers, int totalSuppliers)>(sql);
            return result;
        }




        public async Task<(decimal totalGiven, decimal totalReceived)> GetTransactionTotalsAsync()
        {
            const string sql = @"
        SELECT
            COALESCE(SUM(CASE WHEN type = 'Gave' THEN amount END), 0) AS totalGiven,
            COALESCE(SUM(CASE WHEN type = 'Receive' THEN amount END), 0) AS totalReceived
        FROM credittransactions;
    ";

            return await _db.QueryFirstAsync<(decimal totalGiven, decimal totalReceived)>(sql);
        }



        public async Task<IEnumerable<User>> GetCustomersAsync()
        {
            string sql = "SELECT * FROM users WHERE role = 'customer'";
            return await _db.QueryAsync<User>(sql);
        }





        public async Task<IEnumerable<User>> GetSuppliersAsync()
        {
            string sql = "SELECT * FROM users WHERE role = 'supplier'";
            return await _db.QueryAsync<User>(sql);
        }


    }
}
