using CreditTrack.Application.Service;
using CreditTrack.Domain.Model;
using Microsoft.AspNetCore.Mvc;
using CreditTrack.Application.DTOs;

namespace CreditTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService) => _userService = userService;

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] UserReq user)
        {
            var response = await _userService.CreateUserAndSendEmailAsync(user);
            if (response.Success)
                return Ok(response);
            return BadRequest(response);
        }

        [HttpGet("usernames")]
        public async Task<IActionResult> GetAllUsernames()
        {
            var result = await _userService.GetAllUsernamesAsync();
            return StatusCode(result.Success ? 200 : 400, result);
        }




        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginRequest loginReq)
        //{
        //    var result = await _userService.LoginAsync(loginReq);
        //    if (!result.Success)
        //        return BadRequest(result);

        //    return Ok(result);
        //}

    }
}
