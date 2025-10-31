using CreditTrack.Application.DTOs;
using CreditTrack.Application.Service;
using CreditTrack.Domain.Common; // ApiResponse
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditTrack.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class CreditTransactionController : ControllerBase
    {
        private readonly CreditTransactionService _service;

        public CreditTransactionController(CreditTransactionService service)
        {
            _service = service;
        }

        [HttpPost("gave")]
        public async Task<IActionResult> AddGave([FromBody] CreditTransactionDto dto)
        {



            try
            {
                await _service.AddGaveAsync(dto.UserId, dto.Amount, dto.Description);
                return Ok(ApiResponse<string>.Ok("Gave added successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail("Failed to add gave", new List<string> { ex.Message }));
            }
        }

        [HttpPost("receive")]
        public async Task<IActionResult> AddReceive(CreditTransactionDto dto)
        {
            try
            {
                await _service.AddReceiveAsync(dto.UserId, dto.Amount, dto.Description);
                return Ok(ApiResponse<string>.Ok("Receive added successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail("Failed to add receive", new[] { ex.Message }));
            }
        }

        [HttpGet("balance/{userId}")]
        public async Task<IActionResult> GetUserBalance(int userId)
        {
            try
            {
                var balance = await _service.GetUserBalanceAsync(userId);
                return Ok(ApiResponse<decimal>.Ok(balance, "User balance fetched successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<decimal>.Fail("Failed to fetch user balance", new[] { ex.Message }));
            }
        }

        [HttpGet("balances")]
        public async Task<IActionResult> GetAllUsersBalance()
        {
            try
            {
                var balances = await _service.GetAllUsersBalanceAsync();
                return Ok(ApiResponse<decimal>.Ok(balances, "All users balances fetched successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<object>>.Fail("Failed to fetch all balances", new[] { ex.Message }));
            }
        }



        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserTransactions(int userId)
        {
            var transactions = await _service.GetUserHistoryAsync(userId);
            return Ok(new { success = true, message = "User transaction history fetched successfully", data = transactions });
        }





        [HttpGet("top-users")]
        public async Task<IActionResult> GetTopUsers()
        {
            var result = await _service.GetTopUsersAsync();

            return Ok(new
            {
                success = true,
                message = "Top givers and receivers fetched successfully",
                data = new
                {
                    topGivers = result.topGivers,
                    topReceivers = result.topReceivers
                }
            });

        }
    }
}