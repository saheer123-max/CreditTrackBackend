using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CreditTrack.Application.DTOs;
using CreditTrack.Domain.IRepo;
using CreditTrack.Domain.Model;
namespace CreditTrack.Application.Service
{
  public  class CreditTransactionService
    {

        private readonly ITransactionRepository _repository;

        public CreditTransactionService(  ITransactionRepository repository)
        {
            _repository = repository;
        }

        public async Task<decimal> AddGaveAsync(int userId, decimal amount, string description = "")
        {

            var transaction = new CreditTransaction
            {

                UserId = userId,
                Amount = amount,
                Type = "Gave",
                TransactionDate = DateTime.Now,
                Description = description



            };


            await _repository.AddGaveAsync(transaction);

            var currentBalance = await _repository.GetUserBalanceAsync(userId);
            var newBalance = currentBalance + amount; // Gave -> balance increases
            await _repository.UpdateUserBalanceAsync(userId, newBalance);
            return newBalance;

        }



        public async Task<decimal> AddReceiveAsync(int userId, decimal amount, string description = "")
        {
            var transaction = new CreditTransaction
            {
                UserId = userId,
                Amount = amount,
                Type = "Receive",
                TransactionDate = DateTime.Now,
                Description = description
            };

            await _repository.AddReceiveAsync(transaction);

            // Balance update
            var currentBalance = await _repository.GetUserBalanceAsync(userId);
            var newBalance = currentBalance - amount; // Receive -> balance decreases
            await _repository.UpdateUserBalanceAsync(userId, newBalance);

            return newBalance;
        }


        public async Task<decimal> GetUserBalanceAsync(int userId)
        {
            return await _repository.GetUserBalanceAsync(userId);
        }

        public async Task<decimal> GetAllUsersBalanceAsync()
        {
            return await _repository.GetTotalBalanceAsync();
        }


        public async Task<IEnumerable<CreditTransaction>> GetUserHistoryAsync(int userId)
        {
            return await _repository.GetUserTransactionsAsync(userId);
        }

    }
}
