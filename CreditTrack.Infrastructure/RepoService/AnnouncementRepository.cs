using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;


using Microsoft.Extensions.Configuration;
using CreditTrack.Application.IRepo;
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
            var sql = @"INSERT INTO announcements (message, createdat) 
            VALUES (@Message, @CreatedAt)
            RETURNING id;";



            var id = await _db.QuerySingleAsync<int>(sql, announcement);
                return id;
           
        }

        public async Task<IEnumerable<Announcement>> GetAllAsync()
        {
            var sql = "SELECT * FROM announcements ORDER BY createdat DESC";


            return await _db.QueryAsync<Announcement>(sql);
            
        }
    }
}

