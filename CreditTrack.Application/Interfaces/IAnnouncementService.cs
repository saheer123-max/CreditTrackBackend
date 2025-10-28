using CreditTrack.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditTrack.Application.Interfaces
{
    public interface IAnnouncementService
    {
        Task<Announcement> CreateAnnouncementAsync(string message);
        Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync();
    }
}
