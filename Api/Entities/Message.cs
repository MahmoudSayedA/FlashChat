namespace Api.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ChatId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime RecievedAt { get; set; }
        public Chat? Chat { get; set; }
        public User? Sender { get; set; }

    }
}
