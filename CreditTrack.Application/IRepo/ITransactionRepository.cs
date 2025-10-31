using CreditTrack.Application.DTOs;
using CreditTrack.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditTrack.Application.IRepo
{
  public  interface ITransactionRepository
    {

        Task<IEnumerable<CreditTransaction>> GetUserTransactionsAsync(int userId);
        Task AddGaveAsync(CreditTransaction transaction);
        Task AddReceiveAsync(CreditTransaction transaction);
        Task<decimal> GetUserBalanceAsync(int userId);
        Task UpdateUserBalanceAsync(int userId, decimal newBalance);
        Task<decimal> GetTotalBalanceAsync();

        Task<List<TopUserDto>> GetTopGiversAsync();
        Task<List<TopUserDto>> GetTopReceiversAsync();

    }
}
