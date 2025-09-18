using Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.DbConfigurations
{
    public class GroupChatConfigurations : IEntityTypeConfiguration<GroupChat>
    {
        public void Configure(EntityTypeBuilder<GroupChat> builder)
        {
            builder.Property(gc => gc.Name).IsRequired().HasMaxLength(100);

        }
    }
}
