using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace CreditTrack.Chat
{
    public class ChatHub : Hub
    {
        private static string AdminConnectionId;

        // CustomerId -> ConnectionId mapping
        private static ConcurrentDictionary<string, string> Customers = new();

        // Queue pending messages to admin if admin is not connected yet
        private static ConcurrentQueue<(string senderId, string message)> PendingMessagesToAdmin = new();

        public override async Task OnConnectedAsync()
        {
            var role = Context.GetHttpContext().Request.Query["role"];
            var userId = Context.GetHttpContext().Request.Query["userId"];

            if (role == "admin")
            {
                AdminConnectionId = Context.ConnectionId;

                // എല്ലാം connected customers admin-ന് notify ചെയ്യുക
                foreach (var customer in Customers)
                {
                    await Clients.Client(AdminConnectionId).SendAsync("NewCustomerJoined", customer.Key);
                }
            }
            else
            {
                Customers[userId] = Context.ConnectionId;

                if (!string.IsNullOrEmpty(AdminConnectionId))
                {
                    await Clients.Client(AdminConnectionId).SendAsync("NewCustomerJoined", userId);
                }
            }

            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var role = Context.GetHttpContext().Request.Query["role"];
            var userId = Context.GetHttpContext().Request.Query["userId"];

            Console.WriteLine($"[Disconnected] Role: {role}, UserId: {userId}, ConnectionId: {Context.ConnectionId}");

            if (role == "admin")
            {
                AdminConnectionId = null;
                Console.WriteLine("[Admin Disconnected]");
            }
            else
            {
                Customers.TryRemove(userId, out _);
                Console.WriteLine($"[Customer Disconnected] UserId: {userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Send message from customer to admin or admin to customer
        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            Console.WriteLine($"[SendMessage] SenderId: {senderId}, ReceiverId: {receiverId}, Message: {message}");

            if (receiverId == "admin")
            {
                if (AdminConnectionId != null)
                {
                    Console.WriteLine($"[Deliver to Admin] SenderId: {senderId}");
                    await Clients.Client(AdminConnectionId).SendAsync("ReceiveMessage", senderId, message);
                }
                else
                {
                    Console.WriteLine($"[Queue message to Admin] SenderId: {senderId}");
                    PendingMessagesToAdmin.Enqueue((senderId, message));
                }
            }
            else if (Customers.TryGetValue(receiverId, out var connectionId))
            {
                Console.WriteLine($"[Deliver to Customer] ReceiverId: {receiverId}");
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", senderId, message);
            }
            else
            {
                Console.WriteLine($"[Send Failed] Receiver not connected: {receiverId}");
            }
        }
    }
}
