using Api.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    [Authorize]
    public class ChatHub(
        IPresenceTracker presenceTracker,
        ICurrentUserService currentUserService
        ) : Hub
    {

        public override Task OnConnectedAsync()
        {
            var userId = currentUserService.UserId ?? 0;
            presenceTracker.UserConnected(userId, Context.ConnectionId);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = currentUserService.UserId ?? 0;
            presenceTracker.UserDisconnected(userId, Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }



    }
}
