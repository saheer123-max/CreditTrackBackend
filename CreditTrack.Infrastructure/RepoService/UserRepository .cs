using CreditTrack.Domain.IRepo;
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

        // ✅ Create a new user
        public async Task<int> CreateUserAsync(User user)
        {
            string sql = @"INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt, Role)
                           VALUES (@Username, @Email, @PasswordHash, @IsActive, @CreatedAt, @Role);
                           SELECT CAST(SCOPE_IDENTITY() as int)";
            return await _db.QuerySingleAsync<int>(sql, user);
        }

        // ✅ Get user by email
        public async Task<User> GetUserByEmailAsync(string email)
        {
            string sql = "SELECT * FROM Users WHERE Email = @Email";
            return await _db.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        // ✅ Get user by username
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            string sql = "SELECT * FROM Users WHERE Username = @Username";
            return await _db.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }

        // ✅ Get all users
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            string sql = "SELECT * FROM Users WHERE Role <> 'Admin' ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<User>(sql);
        }






        public async Task<IEnumerable<User>> SearchUsersAsync(string keyword)
        {
            string query = @"SELECT Id, Username, Email 
                             FROM Users 
                             WHERE Username LIKE @Keyword"
            ;

            return await _db.QueryAsync<User>(query, new { Keyword = $"%{keyword}%" });
        }





        public async Task<(int totalCustomers, int totalSuppliers)> GetUserCountsAsync()
        {
            string customerSql = "SELECT COUNT(*) FROM Users WHERE Role = 'customer'";
            string supplierSql = "SELECT COUNT(*) FROM Users WHERE Role = 'supplier'";

            int totalCustomers = await _db.ExecuteScalarAsync<int>(customerSql);
            int totalSuppliers = await _db.ExecuteScalarAsync<int>(supplierSql);

            return (totalCustomers, totalSuppliers);
        }



        public async Task<(decimal totalGiven, decimal totalReceived)> GetTransactionTotalsAsync()
        {
            string totalGivenSql = "SELECT ISNULL(SUM(Amount), 0) FROM CreditTransactions WHERE Type = 'Gave'";
            string totalReceivedSql = "SELECT ISNULL(SUM(Amount), 0) FROM CreditTransactions WHERE Type = 'Receive'";

            decimal totalGiven = await _db.ExecuteScalarAsync<decimal>(totalGivenSql);
            decimal totalReceived = await _db.ExecuteScalarAsync<decimal>(totalReceivedSql);

            return (totalGiven, totalReceived);
        }



        public async Task<IEnumerable<User>> GetCustomersAsync()
        {
            string sql = "SELECT * FROM Users WHERE Role = 'customer'";
            return await _db.QueryAsync<User>(sql);
        }




        public async Task<IEnumerable<User>> GetSuppliersAsync()
        {
            string sql = "SELECT * FROM Users WHERE Role = 'supplier'";
            return await _db.QueryAsync<User>(sql);
        }


    }
}
