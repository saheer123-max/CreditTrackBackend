using Microsoft.AspNetCore.Mvc;
using CreditTrack.Application.DTOs;
using CreditTrack.Application.Interfaces;
using CreditTrack.Domain.Common;
using Microsoft.AspNetCore.Authorization;

namespace CreditTrack.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
     

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;

        }

        /// <summary>
        /// Admin login endpoint
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        /// 

        [AllowAnonymous]

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<LoginResponse>.Fail("Invalid request data."));

            var result = await _adminService.LoginAsync(req);

            if (!result.Success)
                return Unauthorized(result); // failed login returns 401

            return Ok(result); // success returns 200 with token
        }

        /// <summary>
        /// Optional: Seed admin (usually called at startup)
        /// </summary>
        [HttpPost("seed")]

        public async Task<IActionResult> SeedAdmin()
        {
            await _adminService.EnsureSeedAdminAsync();
            return Ok(ApiResponse<string>.Ok("Admin seed completed."));
        }

  



    }
}
