using System.ComponentModel.DataAnnotations;

namespace Api.Application.Dtos
{
    public class UpdateGroupDto
    {
        [MaxLength(50)]
        public string GroupName { get; set; } = string.Empty;
    }
}
