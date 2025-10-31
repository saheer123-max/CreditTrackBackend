using CreditTrack.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CreditTrack.Application.IRepo
{
   public interface IChatRepository
    {
        Task AddMessageAsync(ChatMessage chatMessage);

        Task<IEnumerable<string>> GetAllChatUsersAsync();

        Task<IEnumerable<ChatMessage>> GetChatHistoryAsync(string userId);

    }
}
