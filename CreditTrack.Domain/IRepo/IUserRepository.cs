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

        Task<User> GetUserByUsernameAsync(string username);


        Task<IEnumerable<User>> SearchUsersAsync(string keyword);



        Task<(int totalCustomers, int totalSuppliers)> GetUserCountsAsync();

        Task<(decimal totalGiven, decimal totalReceived)> GetTransactionTotalsAsync();
        Task<IEnumerable<User>> GetCustomersAsync();
        Task<IEnumerable<User>> GetSuppliersAsync();

    }
}
