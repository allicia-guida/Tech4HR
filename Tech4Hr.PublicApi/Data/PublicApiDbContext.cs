using Microsoft.EntityFrameworkCore;
using Tech4Hr.PublicApi.Models;

namespace Tech4Hr.PublicApi.Data;

public sealed class PublicApiDbContext(DbContextOptions<PublicApiDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<PointCorrection> PointCorrections => Set<PointCorrection>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<WorkSchedule> WorkSchedules => Set<WorkSchedule>();
    public DbSet<AbsenceRequest> AbsenceRequests => Set<AbsenceRequest>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Email).IsRequired();
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.Role).IsRequired();
        });

        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.RecordedAtUtc });
            entity.HasIndex(x => new { x.UserId, x.IdempotencyKey }).IsUnique();
            entity.Property(x => x.Type).IsRequired();
            entity.HasOne(x => x.User).WithMany(x => x.TimeEntries).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PointCorrection>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.Status });
            entity.Property(x => x.Reason).IsRequired();
            entity.Property(x => x.Status).IsRequired();
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TimeEntry).WithMany().HasForeignKey(x => x.TimeEntryId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.Property(x => x.TokenHash).IsRequired();
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkSchedule>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.DayOfWeek }).IsUnique();
            entity.Property(x => x.TimeZoneId).IsRequired();
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AbsenceRequest>(entity =>
        {
            entity.HasIndex(x => new { x.UserId, x.Status });
            entity.Property(x => x.Type).IsRequired();
            entity.Property(x => x.Reason).IsRequired();
            entity.Property(x => x.Status).IsRequired();
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.HasIndex(x => new { x.Date, x.StateCode, x.City }).IsUnique();
            entity.Property(x => x.Name).IsRequired();
        });

        modelBuilder.Entity<AuditEvent>(entity =>
        {
            entity.HasIndex(x => x.CreatedAtUtc);
            entity.Property(x => x.Action).IsRequired();
            entity.Property(x => x.EntityType).IsRequired();
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasIndex(x => x.UserId);
            entity.Property(x => x.OriginalName).IsRequired();
            entity.Property(x => x.StoredName).IsRequired();
            entity.Property(x => x.ContentType).IsRequired();
            entity.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<PointCorrection>().WithMany().HasForeignKey(x => x.PointCorrectionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<AbsenceRequest>().WithMany().HasForeignKey(x => x.AbsenceRequestId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
