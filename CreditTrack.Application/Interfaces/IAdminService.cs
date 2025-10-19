using CreditTrack.Application.DTOs;

using CreditTrack.Domain.Common;

namespace CreditTrack.Application.Interfaces
{
   public interface IAdminService
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest req);
        Task EnsureSeedAdminAsync();
    }
}
