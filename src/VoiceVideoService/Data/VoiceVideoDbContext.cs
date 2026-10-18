namespace VoiceVideoService.Data;

using Microsoft.EntityFrameworkCore;
using VoiceVideoService.Messages;
using VoiceVideoService.Models;

public class VoiceVideoDbContext : DbContext
{
    public VoiceVideoDbContext(
        DbContextOptions<VoiceVideoDbContext> options)
        : base(options)
    {
    }

    public DbSet<CallSession> CallSessions => Set<CallSession>();

    public DbSet<CallParticipant> CallParticipants => Set<CallParticipant>();

    public DbSet<VoiceVideoProcessedMessage> ProcessedMessages => Set<VoiceVideoProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CallSession>(entity =>
        {
            entity.HasKey(session => session.Id);
            entity.Property(session => session.CallStatus)
                .HasMaxLength(32)
                .IsRequired();
            entity.HasIndex(session => new { session.ChannelId, session.CallStatus });

            entity.HasMany(session => session.Participants)
                .WithOne(participant => participant.CallSession)
                .HasForeignKey(participant => participant.CallSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CallParticipant>(entity =>
        {
            entity.HasKey(participant => participant.Id);
            entity.HasIndex(participant => new { participant.CallSessionId, participant.UserId });
        });

        modelBuilder.Entity<VoiceVideoProcessedMessage>(entity =>
        {
            entity.HasKey(message => message.MessageId);
            entity.HasIndex(message => message.SessionId);
        });
    }
}
