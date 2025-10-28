using CreditTrack.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CreditTrack.Domain.IRepo
{
  public  interface IAnnouncementRepository
    {
        Task<int> CreateAsync(Announcement announcement);
        Task<IEnumerable<Announcement>> GetAllAsync();
    }
}
