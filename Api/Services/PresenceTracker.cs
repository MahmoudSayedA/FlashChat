using Api.Application.Abstractions;

namespace Api.Services
{
    public class PresenceTracker : IPresenceTracker
    {
        private static readonly Dictionary<int, HashSet<string>> _onlineUsers = [];
        public IEnumerable<string> GetConnections(int targetUserId)
        {
            var connections = _onlineUsers.GetValueOrDefault(targetUserId);
            return connections ?? Enumerable.Empty<string>();

        }

        public async Task<List<int>> GetOnlineUsers()
        {
            // { userId, {con2, con1, con3}}
            var onlineUsers = _onlineUsers
                .Where(connections => connections.Value.Count > 0)
                .SelectMany(kvp => kvp.Value)
                .Select(connectionId => _onlineUsers.FirstOrDefault(kvp => kvp.Value.Contains(connectionId)).Key)
                .Distinct()
                .ToList();

            return await Task.FromResult(onlineUsers);

        }

        public Task<bool> IsUserOnline(int userId)
        {
            return Task.FromResult(_onlineUsers.ContainsKey(userId) && _onlineUsers[userId].Count > 0);
        }

        public Task UserConnected(int userId, string connectionId)
        {
            if (_onlineUsers.TryGetValue(userId, out HashSet<string>? value))
            {
                value.Add(connectionId);
            }
            else
            {
                _onlineUsers[userId] = new HashSet<string> { connectionId };
            }
            return Task.CompletedTask;
        }

        public Task UserDisconnected(int userId, string connectionId)
        {
            if(_onlineUsers.TryGetValue(userId, out HashSet<string>? value))
            {
                value.Remove(connectionId);
                if(value.Count == 0)
                {
                    _onlineUsers.Remove(userId);
                }
            }
            return Task.CompletedTask;
        }
    }
}
