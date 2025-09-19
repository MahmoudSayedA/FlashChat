namespace Api.Entities
{
    public abstract class Chat
    {
        public int Id { get; set; }
        public abstract string Type { get; internal set; }
        public int CreatedById { get; set; }
        public User? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<Message> Messages { get; set; } = [];
    }

    public class PrivateChat : Chat
    {
        public override string Type { get; internal set; } = ChatTypes.Private;
        public int User1Id { get; set; }
        public int User2Id { get; set; }
        public User? User1 { get; set; }
        public User? User2 { get; set; }
    }

    public class GroupChat : Chat
    {
        public override string Type { get; internal set; } = ChatTypes.Group;
        public required string Name { get; set; }
        public ICollection<GroupChatMember> Members { get; set; } = [];
    }

    public static class ChatTypes
    {
        public const string Private = "Private";
        public const string Group = "Group";
    }
}
