using Api.Application.Abstractions;
using Api.Application.Dtos;
using Api.Application.Models;
using Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    [Authorize]
    public class ChatHub(
        IPresenceTracker presenceTracker,
        IDbContext dbContext,
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

        public async Task SendMessage(AddMessageDto messageDto)
        {
            var currentUserId = currentUserService.UserId ?? 0;
            Message message = new()
            {
                SenderId = currentUserId,
                Content = messageDto.Content,
                SentAt = DateTime.UtcNow,
                ChatId = messageDto.ChatId,
            };

            using var transaction = await dbContext.BeginTransactionAsync();
            try
            {
                // get target audience
                var chat = await dbContext.Set<Chat>()
                    .FindAsync(messageDto.ChatId)
                    ?? throw new Exception("Chat not found");

                List<int> targetUserIds = [];
                if (chat is PrivateChat privateChat)
                {
                    targetUserIds.Add(privateChat.User1Id);
                    targetUserIds.Add(privateChat.User2Id);
                }
                else if (chat is GroupChat groupChat)
                {
                    var members = dbContext.Set<GroupChatMember>()
                        .Where(m => m.GroupChatId == groupChat.Id);/// TODO: optimize

                    targetUserIds.AddRange(members.Select(m => m.UserId).ToList());
                }
                targetUserIds = targetUserIds.Distinct().Where(id => id != currentUserId).ToList();

                // send to online users
                foreach (var targetUserId in targetUserIds)
                {
                    var connections = presenceTracker.GetConnections(targetUserId);
                    if (connections != null)
                    {
                        foreach (var connectionId in connections)
                        {
                            var messageModel = new MessageModel
                            {
                                Content = message.Content,
                                SentAt = message.SentAt,
                                Id = message.Id,
                                Sender = new UserDto
                                {
                                    Id = currentUserId,
                                    Email = currentUserService.Email ?? "",
                                    Username = currentUserService.UserName ?? ""
                                }
                            };
                            await Clients.Client(connectionId).SendAsync("NewMessage", currentUserId, messageModel);
                        }
                    }
                }

                // Save message to database
                await dbContext.Set<Message>().AddAsync(message);
                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



    }
}
