using CreditTrack.Application.DTOs;
using CreditTrack.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditTrack.Application.Interfaces
{
    public interface IChatService
    {
        Task<ApiResponse<string>> SaveMessageAsync(ChatMessageDto dto);
        Task<IEnumerable<string>> GetAllChatUsersAsync();
        Task<IEnumerable<ChatMessageDto>> GetChatHistoryAsync(string userId);

    }
}
