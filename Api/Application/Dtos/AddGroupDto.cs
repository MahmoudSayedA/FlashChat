using System.ComponentModel.DataAnnotations;

namespace Api.Application.Dtos
{
    public class AddGroupDto
    {
        [MaxLength(50)]
        public string GroupName { get; set; } = string.Empty;

        public List<string> MembersEmails { get; set; } = new();
    }
}
