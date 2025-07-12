using Entities.Models;  // Entity'lerin bulunduğu namespace
using Microsoft.EntityFrameworkCore;

public class ProjectConnect : DbContext
{
    public ProjectConnect(DbContextOptions<ProjectConnect> options)
        : base(options)
    {
    }

    // Kullanıcı tablosu
    public DbSet<User> Users { get; set; }

    // Toplantı tablosu
    public DbSet<Meeting> Meetings { get; set; }

    // Toplantı katılımcısı tablosu
    public DbSet<MeetingParticipant> MeetingParticipants { get; set; }

    // Model ilişkilerini tanımlamak için OnModelCreating kullanıyoruz
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User - Meeting ilişkisi
        modelBuilder.Entity<Meeting>()
            .HasOne(m => m.User)
            .WithMany(u => u.Meetings)
            .HasForeignKey(m => m.UserId);

        // Meeting - MeetingParticipant ilişkisi
        // Meeting - MeetingParticipant ilişkisi
        modelBuilder.Entity<MeetingParticipant>()
            .HasOne(mp => mp.Meeting)
            .WithMany(m => m.MeetingParticipants)
            .HasForeignKey(mp => mp.MeetingId)
            .OnDelete(DeleteBehavior.NoAction);  // NoAction kullanıldı, silme kısıtlaması yapılmaz

        // MeetingParticipant - User ilişkisi
        modelBuilder.Entity<MeetingParticipant>()
            .HasOne(mp => mp.User)
            .WithMany(u => u.MeetingParticipants)
            .HasForeignKey(mp => mp.UserId)
            .OnDelete(DeleteBehavior.Restrict);  // Restrict kullanıldı, bağlı veriler silinemez

        // Global query filter: Sadece silinmemiş kullanıcılar
        modelBuilder.Entity<User>()
            .HasQueryFilter(u => !u.IsDeleted);
    }

}
