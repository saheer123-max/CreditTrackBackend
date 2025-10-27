using CreditTrack.Domain.IRepo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreditTrack.Domain.Model;
using Dapper;

namespace CreditTrack.Infrastructure.RepoService
{
  public  class ChatRepository:IChatRepository
    {

        private readonly IDbConnection _db;

            public ChatRepository(IDbConnection db)
        {
            _db = db;
        }




        public async Task AddMessageAsync(ChatMessage message)
        {
            var sql = @"INSERT INTO ChatMessages (SenderId, ReceiverId, Message, CreatedAt)
                        VALUES (@SenderId, @ReceiverId, @Message, @CreatedAt)";
            await _db.ExecuteAsync(sql, message);
        }



        public async Task<IEnumerable<string>> GetAllChatUsersAsync()
        {
            string sql = @"
    SELECT DISTINCT
        CASE
            WHEN SenderId = '26' THEN ReceiverId
            ELSE SenderId
        END AS UserId
    FROM ChatMessages
    WHERE SenderId = '26' OR ReceiverId = '26'
";


            var users = await _db.QueryAsync<string>(sql);
            return users;
        }




        public async Task<IEnumerable<ChatMessage>> GetChatHistoryAsync(string userId)
        {
            string sql = @"
                SELECT * FROM ChatMessages
                WHERE (SenderId = @UserId AND ReceiverId = '26')
                   OR (SenderId = '26' AND ReceiverId = @UserId)
                ORDER BY CreatedAt ASC
            ";

            var messages = await _db.QueryAsync<ChatMessage>(sql, new { UserId = userId });
            return messages;
        }
    }
}
