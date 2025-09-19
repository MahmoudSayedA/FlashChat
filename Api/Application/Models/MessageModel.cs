using Api.Application.Auth.Dtos;
using Api.Application.Dtos;

namespace Api.Application.Models
{
    public class MessageModel
    {
        public int Id { get; set; }
        public UserDto? Sender { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
