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





        [HttpGet("user-counts")]
        public async Task<IActionResult> GetUserCounts()
        {
            var result = await _userService.GetUserCountsAsync();
            return Ok(result);
        }


        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _userService.GetCustomersAsync();
            return Ok(customers);
        }

        [HttpGet("suppliers")]
        public async Task<IActionResult> GetSuppliers()
        {
            var suppliers = await _userService.GetSuppliersAsync();
            return Ok(suppliers);
        }



        [HttpGet("totals")]
        public async Task<IActionResult> GetTransactionTotals()
        {
            var result = await _userService.GetTransactionTotalsAsync();
            return Ok(result);
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
