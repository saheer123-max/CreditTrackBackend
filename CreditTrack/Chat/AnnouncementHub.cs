using CreditTrack.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace YourProject.WebAPI.Hubs
{
    public class AnnouncementHub : Hub
    {
        private readonly IAnnouncementService _announcementService;

        public AnnouncementHub(IAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }

        public async Task SendAnnouncement(string message)
        {
            var saved = await _announcementService.CreateAnnouncementAsync(message);

           
            await Clients.All.SendAsync("ReceiveAnnouncement", saved.Message);
        }
    }
}
