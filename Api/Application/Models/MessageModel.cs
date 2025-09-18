namespace Api.Application.Models
{
    public class MessageModel
    {
        public int Id { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string Reciever { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime RecievedAt { get; set; }

    }
}
