using Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.DbConfigurations
{
    public class PrivateChatConfigurations : IEntityTypeConfiguration<PrivateChat>
    {
        public void Configure(EntityTypeBuilder<PrivateChat> builder)
        {

            builder.HasOne(pc => pc.User1)
                   .WithMany()
                   .HasForeignKey(pc => pc.User1Id)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pc => pc.User2)
                   .WithMany()
                   .HasForeignKey(pc => pc.User2Id)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pc => new { pc.User1Id, pc.User2Id }).IsUnique();
            builder.HasIndex(pc => new { pc.User2Id, pc.User1Id }).IsUnique();
            builder.HasIndex(pc => new { pc.User1Id });
            builder.HasIndex(pc => new { pc.User2Id });
        }
    }

}
