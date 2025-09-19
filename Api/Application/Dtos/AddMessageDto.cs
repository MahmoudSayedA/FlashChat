using Api.Entities;

namespace Api.Application.Dtos
{
    public class AddMessageDto
    {
        public string Content { get; set; } = string.Empty;
        public int ChatId { get; set; }
    }
}
