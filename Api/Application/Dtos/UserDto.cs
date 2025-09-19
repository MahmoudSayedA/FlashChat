namespace Api.Application.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public static UserDto FromEntity(Entities.User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                CreatedAt = user.CreatedAt,
            };
        }
    }
}
