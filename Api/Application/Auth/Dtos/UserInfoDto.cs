namespace Api.Application.Auth.Dtos
{
    public class UserInfoDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public List<string> Roles { get; set; } = new();
        public Dictionary<string, string> Claims { get; set; } = new();
    }

}
