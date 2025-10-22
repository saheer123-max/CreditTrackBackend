using CreditTrack.Domain.IRepo;
using CreditTrack.Domain.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
namespace CreditTrack.Infrastructure.Services 
{
  public  class UserRepository : IUserRepository 
    {
        private readonly IDbConnection _db;
        public UserRepository(IDbConnection db) => _db = db;

        public async Task<int> CreateUserAsync(User user)
        {
            string sql = @"INSERT INTO Users (Username, Email, PasswordHash, IsActive, CreatedAt,Role)
                       VALUES (@Username, @Email, @PasswordHash, @IsActive, @CreatedAt,@Role);
                       SELECT CAST(SCOPE_IDENTITY() as int)";
            return await _db.QuerySingleAsync<int>(sql, user);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            string sql = "SELECT * FROM Users WHERE Email = @Email";
            return await _db.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            string sql = "SELECT * FROM Users ORDER BY CreatedAt DESC";
            return await _db.QueryAsync<User>(sql);
        }

 

    }
}
