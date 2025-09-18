namespace Api.Application.Abstractions
{
    public interface IPresenceTracker
    {
        public Task UserConnected(int userId, string connectionId);
        public Task UserDisconnected(int userId, string connectionId);
        public Task<bool> IsUserOnline(int userId);
        public Task<List<int>> GetOnlineUsers();

    }
}
