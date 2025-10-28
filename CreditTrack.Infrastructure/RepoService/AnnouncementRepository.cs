using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;
using Microsoft.Data.SqlClient;

using Microsoft.Extensions.Configuration;
using CreditTrack.Domain.IRepo;
using CreditTrack.Domain.Model;
using System.Data;

namespace YourProject.Infrastructure.Repositories
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly IDbConnection _db;

        public AnnouncementRepository(IDbConnection db)
        {
            _db = db;
        }

     

        public async Task<int> CreateAsync(Announcement announcement)
        {
            var sql = @"INSERT INTO Announcements (Message, CreatedAt) 
                        VALUES (@Message, @CreatedAt);
                        SELECT CAST(SCOPE_IDENTITY() as int);";

            
            
                var id = await _db.QuerySingleAsync<int>(sql, announcement);
                return id;
           
        }

        public async Task<IEnumerable<Announcement>> GetAllAsync()
        {
            var sql = "SELECT * FROM Announcements ORDER BY CreatedAt DESC";
              
                       
                return await _db.QueryAsync<Announcement>(sql);
            
        }
    }
}

