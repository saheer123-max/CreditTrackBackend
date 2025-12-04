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
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace CreditTrack.Infrastructure.RepoService
{
  public  class CreditTransactionRepository:ITransactionRepository
    {


        private readonly IDbConnection _db;

        public CreditTransactionRepository(IDbConnection db) => _db = db;

        public async Task AddGaveAsync(CreditTransaction transaction)
        {
            string sql = @"
        INSERT INTO credittransactions
        (userid, amount, transactiondate, type, description)
        VALUES (@UserId, @Amount, @TransactionDate, 'Gave', @Description);
    ";

            await _db.ExecuteAsync(sql, transaction);
        }

        public async Task AddReceiveAsync(CreditTransaction transaction)
        {
            string sql = @"
        INSERT INTO credittransactions
        (userid, amount, transactiondate, type, description)
        VALUES (@UserId, @Amount, @TransactionDate, 'Receive', @Description);
    ";

            await _db.ExecuteAsync(sql, transaction);
        }

        public async Task<decimal> GetUserBalanceAsync(int userId)
        {
            string sql = @"
        SELECT COALESCE(balance, 0)
        FROM userbalance
        WHERE userid = @userId;
    ";

            return await _db.ExecuteScalarAsync<decimal>(sql, new { userId });
        }


        public async Task UpdateUserBalanceAsync(int userId, decimal newBalance)
        {
            string sql = @"
        INSERT INTO userbalance (userid, balance)
        VALUES (@UserId, @Balance)
        ON CONFLICT (userid)
        DO UPDATE SET balance = @Balance;
    ";

            await _db.ExecuteAsync(sql, new { UserId = userId, Balance = newBalance });
        }



        public async Task<decimal> GetTotalBalanceAsync()
        {
            string sql = @"SELECT SUM(""Balance"") FROM ""UserBalance"";";
            return await _db.ExecuteScalarAsync<decimal>(sql);
        }

        public async Task<IEnumerable<CreditTransaction>> GetUserTransactionsAsync(int userId)
        {
            string sql = @"
        SELECT *
        FROM credittransactions
        WHERE userid = @UserId
        ORDER BY transactiondate DESC;
    ";

            return await _db.QueryAsync<CreditTransaction>(sql, new { UserId = userId });
        }




        public async Task<List<TopUserDto>> GetTopGiversAsync()
        {
            string query = @"
        SELECT 
            u.id AS userid,
            u.username,
            SUM(c.amount) AS totalamount
        FROM credittransactions c
        JOIN users u ON c.userid = u.id
        WHERE c.type = 'Gave'
        GROUP BY u.id, u.username
        ORDER BY totalamount DESC
        LIMIT 3;
    ";

            var result = await _db.QueryAsync<TopUserDto>(query);
            return result.AsList();
        }


        public async Task<List<TopUserDto>> GetTopReceiversAsync()
        {
            string query = @"
        SELECT 
            u.id AS userid,
            u.username,
            SUM(c.amount) AS totalamount
        FROM credittransactions c
        JOIN users u ON c.userid = u.id
        WHERE c.type = 'Receive'
        GROUP BY u.id, u.username
        ORDER BY totalamount DESC
        LIMIT 3;
    ";

            var result = await _db.QueryAsync<TopUserDto>(query);
            return result.AsList();
        }

















    }
}
