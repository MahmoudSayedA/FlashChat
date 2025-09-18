using Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.DbConfigurations
{
    public class GroupChatMemberConfigurations : IEntityTypeConfiguration<GroupChatMember>
    {
        public void Configure(EntityTypeBuilder<GroupChatMember> builder)
        {
            builder.ToTable(nameof(GroupChatMember));
            builder.HasKey(gcm => new { gcm.GroupChatId, gcm.UserId });

            builder.HasOne(gcm => gcm.GroupChat)
                   .WithMany(gc => gc.Members)
                   .HasForeignKey(gcm => gcm.GroupChatId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(gcm => gcm.User)
                   .WithMany()
                   .HasForeignKey(gcm => gcm.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
