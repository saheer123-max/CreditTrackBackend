using CreditTrack.Application.DTOs;
using CreditTrack.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _chatService.GetAllChatUsersAsync();
        return Ok(users);
    }

    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetChatHistory(string userId)
    {
        var history = await _chatService.GetChatHistoryAsync(userId);
        return Ok(history);
    }

}
