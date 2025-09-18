using Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.DbConfigurations
{
    public class ChatConfigurations : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder.HasDiscriminator(c => c.Type)
                   .HasValue<PrivateChat>(ChatTypes.Private)
                   .HasValue<GroupChat>(ChatTypes.Group);

            builder.Property(c => c.CreatedAt).HasDefaultValueSql("GETDATE()");

            builder.HasOne(c => c.CreatedBy)
                   .WithMany()
                   .HasForeignKey(c => c.CreatedById)
                   .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
