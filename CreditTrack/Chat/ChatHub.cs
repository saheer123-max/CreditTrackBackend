using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CreditTrack.Application.DTOs;
using CreditTrack.Application.Interfaces;

namespace CreditTrack.Chat
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        // 🟢 Static variables to store connected users
        private static string AdminConnectionId;
        private static readonly ConcurrentDictionary<string, string> Customers = new();

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var role = httpContext?.Request.Query["role"].ToString();
            var userId = httpContext?.Request.Query["userId"].ToString();

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userId))
            {
                await base.OnConnectedAsync();
                return;
            }

            // ✅ Store connection IDs
            if (role == "admin")
            {
                AdminConnectionId = Context.ConnectionId;
                Console.WriteLine($"✅ Admin connected: {AdminConnectionId}");
            }
            else
            {
                Customers[userId] = Context.ConnectionId;
                Console.WriteLine($"🟢 Customer connected: {userId}");

                // 🔔 Notify admin that a new customer joined
                if (AdminConnectionId != null)
                {
                    await Clients.Client(AdminConnectionId).SendAsync("NewCustomerJoined", userId);
                }

             
            }

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId) || string.IsNullOrEmpty(message))
                return;

            var chatMessage = new ChatMessageDto
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message
            };

            // 💾 Save message in DB
            await _chatService.SaveMessageAsync(chatMessage);

            // 🟡 Send to Admin
            if (receiverId == "admin" && AdminConnectionId != null)
            {
                await Clients.Client(AdminConnectionId).SendAsync("ReceiveMessage", senderId, message);
            }
            // 🟢 Send to Customer
            else if (Customers.TryGetValue(receiverId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", senderId, message);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // ❌ Remove disconnected customer
            var disconnectedUser = Customers.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(disconnectedUser.Key))
            {
                Customers.TryRemove(disconnectedUser.Key, out _);
                Console.WriteLine($"🔴 Customer disconnected: {disconnectedUser.Key}");
            }

            // ❌ Remove admin connection if admin disconnected
            if (AdminConnectionId == Context.ConnectionId)
            {
                AdminConnectionId = null;
                Console.WriteLine("⚠️ Admin disconnected");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
