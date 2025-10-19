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
  public  class CreditTransactionRepository:ITransactionRepository
    {


        private readonly IDbConnection _db;

        public CreditTransactionRepository(IDbConnection db) => _db = db;

        public async Task AddGaveAsync(CreditTransaction transaction)
        {
            string sql = @"INSERT INTO  CreditTransactions
                         (UserId, Amount, TransactionDate, Type, Description)
                         VALUES (@UserId, @Amount, @TransactionDate, 'Gave', @Description)";

             await _db.ExecuteAsync(sql, transaction);
        }


        public  async Task AddReceiveAsync(CreditTransaction transaction)
        {
            string sql = @"INSERT INTO CreditTransactions 
                      (UserId, Amount, TransactionDate, Type, Description)
                       VALUES (@UserId, @Amount, @TransactionDate, 'Receive', @Description)";

            await _db.ExecuteAsync(sql, transaction);
  
        }

        public async  Task <decimal> GetUserBalanceAsync(int userid)
        {

            string sql = @"SELECT  ISNULL(Balance,0)FROM UserBalance WHERE userId=@userId";

            return await _db.ExecuteScalarAsync<decimal>(sql, new { userId = userid });
        }


        public async Task UpdateUserBalanceAsync(int userId, decimal newBalance)
        {

            string sql = @"
                IF EXISTS (SELECT 1 FROM UserBalance WHERE UserId = @UserId)
                    UPDATE UserBalance SET Balance = @Balance WHERE UserId = @UserId
                ELSE
                    INSERT INTO UserBalance(UserId, Balance) VALUES(@UserId, @Balance)";


            await _db.ExecuteAsync(sql, new { UserId = userId, Balance = newBalance });


        }


        public async Task<decimal> GetTotalBalanceAsync()
        {
          
            string sql = "SELECT SUM(Balance) FROM UserBalance";
            return await _db.ExecuteScalarAsync<decimal>(sql);
        }

        public async Task<IEnumerable<CreditTransaction>> GetUserTransactionsAsync(int userId)
        {
            string sql = @"SELECT * FROM CreditTransactions 
                           WHERE UserId = @UserId
                           ORDER BY TransactionDate DESC";

            return await _db.QueryAsync<CreditTransaction>(sql, new { UserId = userId });
        }

    }
}
