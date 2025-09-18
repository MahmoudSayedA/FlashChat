namespace Api.Entities
{
    public class GroupChatMember
    {
        public int Id { get; set; }
        public int GroupChatId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public string Role { get; set; } = Constants.Roles.Member; // e.g., Admin, Member
        public GroupChat? GroupChat { get; set; }
        public User? User { get; set; }
    }
}