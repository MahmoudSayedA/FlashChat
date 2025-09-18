using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public Task SendMessage(string reciever, string message)
        {
            return Clients.All.SendAsync("newMessage", reciever, message);
        }

    }
}
