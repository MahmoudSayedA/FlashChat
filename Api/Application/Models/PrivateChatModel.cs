using Api.Application.Dtos;

namespace Api.Application.Models
{
    public class PrivateChatModel
    {
        public int Id { get; set; }
        public UserDto? User1 { get; set; }
        public UserDto? User2 { get; set; }
        public MessageModel? LastMessage { get; set; }
        public DateTime CreatedAt { get; set; }

        public static PrivateChatModel FromEntity(Entities.PrivateChat privateChat, MessageModel? lastMessage = null)
        {
            return new PrivateChatModel
            {
                Id = privateChat.Id,
                User1 = privateChat.User1 != null ? UserDto.FromEntity(privateChat.User1) : null,
                User2 = privateChat.User2 != null ? UserDto.FromEntity(privateChat.User2) : null,
                LastMessage = lastMessage,
                CreatedAt = privateChat.CreatedAt,
            };
        }
    }

}
