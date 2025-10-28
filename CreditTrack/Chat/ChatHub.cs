using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CreditTrack.Application.DTOs;
using CreditTrack.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CreditTrack.Chat
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatHub> _logger;

        // 🟢 Static variables to store connected users
        private static string AdminConnectionId;
        private static readonly ConcurrentDictionary<string, string> Customers = new();
        private static string AdminUserId;

        public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        // 🟢 When user connects
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var role = httpContext?.Request.Query["role"].ToString();
            var userId = httpContext?.Request.Query["userId"].ToString();

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("⚠️ Connection missing role or userId");
                await base.OnConnectedAsync();
                return;
            }

            // ✅ Store connection IDs
            if (role == "admin")
            {
                AdminConnectionId = Context.ConnectionId;
                AdminUserId = userId;
                _logger.LogInformation("✅ Admin connected: {ConnectionId} (AdminId: {UserId})", AdminConnectionId, AdminUserId);
            }
            else
            {
                Customers[userId] = Context.ConnectionId;
                _logger.LogInformation("🟢 Customer connected: {UserId}", userId);

                if (AdminConnectionId != null)
                {
                    await Clients.Client(AdminConnectionId).SendAsync("NewCustomerJoined", userId);
                }
            }

            await base.OnConnectedAsync();
        }

        // 🟢 Send message between admin and customer
        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId) || string.IsNullOrEmpty(message))
                {
                    _logger.LogWarning("⚠️ SendMessage called with empty values. Sender={SenderId}, Receiver={ReceiverId}, Message={Message}", senderId, receiverId, message);
                    return;
                }

                _logger.LogInformation("📩 SendMessage called: Sender={SenderId}, Receiver={ReceiverId}, Message={Message}", senderId, receiverId, message);

                var chatMessage = new ChatMessageDto
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Message = message
                };

                await _chatService.SaveMessageAsync(chatMessage);

                // ✅ Deliver message to target
                if (receiverId == AdminUserId && AdminConnectionId != null)
                {
                    _logger.LogInformation("📤 Sending message to admin ({AdminUserId})", AdminUserId);
                    await Clients.Client(AdminConnectionId).SendAsync("ReceiveMessage", senderId, message);
                }
                else if (Customers.TryGetValue(receiverId, out var connectionId))
                {
                    _logger.LogInformation("📤 Sending message to customer {ReceiverId}", receiverId);
                    await Clients.Client(connectionId).SendAsync("ReceiveMessage", senderId, message);
                }
                else
                {
                    _logger.LogWarning("⚠️ No active SignalR connection found for receiver {ReceiverId}", receiverId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ SendMessage Error - Sender={SenderId}, Receiver={ReceiverId}", senderId, receiverId);
            }
        }

        // 🛑 When a user disconnects
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var disconnectedUser = Customers.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(disconnectedUser.Key))
            {
                Customers.TryRemove(disconnectedUser.Key, out _);
                _logger.LogInformation("🔴 Customer disconnected: {UserId}", disconnectedUser.Key);
            }

            if (AdminConnectionId == Context.ConnectionId)
            {
                _logger.LogWarning("⚠️ Admin disconnected: {AdminUserId}", AdminUserId);
                AdminConnectionId = null;
                AdminUserId = null;
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}