using CreditTrack.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace YourProject.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnnouncementController : ControllerBase
    {
        private readonly IAnnouncementService _service;

        public AnnouncementController(IAnnouncementService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var announcements = await _service.GetAllAnnouncementsAsync();
            return Ok(announcements);
        }
    }
}
