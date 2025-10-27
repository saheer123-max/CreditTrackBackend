using CreditTrack.Domain.IRepo;
using Microsoft.AspNetCore.SignalR;


public class SearchHub : Hub
{
    private readonly IUserRepository _userRepository;

    public SearchHub(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task SearchUsers(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            await Clients.Caller.SendAsync("ReceiveSearchResults", new List<object>());
            return;
        }

        var users = await _userRepository.SearchUsersAsync(keyword);
        await Clients.Caller.SendAsync("ReceiveSearchResults", users);
    }
}
