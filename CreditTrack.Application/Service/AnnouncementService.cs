using CreditTrack.Application.Interfaces;
using CreditTrack.Application.IRepo;
using CreditTrack.Domain.Model;



namespace YourProject.Application.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _repository;

        public AnnouncementService(IAnnouncementRepository repository)
        {
            _repository = repository;
        }

        public async Task<Announcement> CreateAnnouncementAsync(string message)
        {
            var newAnnouncement = new Announcement
            {
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            var id = await _repository.CreateAsync(newAnnouncement);
            newAnnouncement.Id = id;

            return newAnnouncement;
        }

        public async Task<IEnumerable<Announcement>> GetAllAnnouncementsAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
