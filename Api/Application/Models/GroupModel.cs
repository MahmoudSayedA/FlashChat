using Api.Application.Dtos;

namespace Api.Application.Models
{
    public class GroupModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public MessageModel? LastMessage { get; set; }
        public DateTime CreatedAt { get; set; }

        public static GroupModel FromEntity(Entities.GroupChat groupChat, MessageModel? lastMessage = null)
        {
            return new GroupModel
            {
                Id = groupChat.Id,
                Name = groupChat.Name,
                LastMessage = lastMessage,
                CreatedAt = groupChat.CreatedAt,
            };
        }
    }
}
