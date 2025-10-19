using CreditTrack.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditTrack.Domain.IRepo
{
  public  interface IUserRepository
    {
        Task<int> CreateUserAsync(User user);
        Task<User> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
    }
}
