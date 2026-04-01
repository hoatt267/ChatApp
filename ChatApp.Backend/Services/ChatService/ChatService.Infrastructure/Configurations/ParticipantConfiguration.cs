using ChatService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Infrastructure.Configurations
{
    public class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
    {
        public void Configure(EntityTypeBuilder<Participant> builder)
        {
            builder.ToTable("Participants");
            builder.HasKey(p => p.Id);

            // Cấu hình Relationship & Cascade Delete
            builder.HasOne(p => p.Conversation)
                   .WithMany(c => c.Participants)
                   .HasForeignKey(p => p.ConversationId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}