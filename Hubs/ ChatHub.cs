using Microsoft.AspNetCore.SignalR;

namespace ChatApplication.Hubs
{
    public class ChatHub : Hub
    {
        // Mesaj gönderildiğinde tetiklenen metod
        public async Task SendMessage(int senderId, int? receiverId, int? groupId, string messageText)
        {
            if (groupId.HasValue)
            {
                // Grup mesajı gönder
                await Clients.Group($"Group_{groupId.Value}")
                    .SendAsync("ReceiveMessage", senderId, messageText);
            }
            else if (receiverId.HasValue)
            {
                // Kullanıcıya özel mesaj gönder
                await Clients.User(receiverId.Value.ToString())
                    .SendAsync("ReceiveMessage", senderId, messageText);
            }
        }

        // Kullanıcı gruba katıldığında tetiklenen metod
        public async Task JoinGroup(int groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Group_{groupId}");
        }

        // Kullanıcı gruptan ayrıldığında tetiklenen metod
        public async Task LeaveGroup(int groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Group_{groupId}");
        }
    }
}
