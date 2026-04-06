using LanManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LanManager.Data;

public class LanManagerDbContext(DbContextOptions<LanManagerDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<CheckInRecord> CheckInRecords => Set<CheckInRecord>();
    public DbSet<DoorPassRecord> DoorPasses => Set<DoorPassRecord>();
    public DbSet<Seat> Seats => Set<Seat>();

    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<TournamentParticipant> TournamentParticipants => Set<TournamentParticipant>();
    public DbSet<TournamentMatch> TournamentMatches => Set<TournamentMatch>();

    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<EquipmentLoan> EquipmentLoans => Set<EquipmentLoan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Registration>()
            .HasIndex(r => new { r.EventId, r.UserId })
            .IsUnique();

        modelBuilder.Entity<CheckInRecord>()
            .HasIndex(c => new { c.EventId, c.UserId });

        modelBuilder.Entity<Event>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Registration>()
            .Property(r => r.Status)
            .HasConversion<string>();

        modelBuilder.Entity<DoorPassRecord>()
            .Property(d => d.Direction)
            .HasConversion<string>();

        modelBuilder.Entity<DoorPassRecord>()
            .HasIndex(d => new { d.EventId, d.UserId, d.ScannedAt });

        modelBuilder.Entity<Seat>(b => {
            b.HasKey(s => s.Id);
            b.HasIndex(s => new { s.EventId, s.Row, s.Column }).IsUnique();
            b.HasOne(s => s.Event).WithMany().HasForeignKey(s => s.EventId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Tournament>(b => {
            b.HasKey(t => t.Id);
            b.HasIndex(t => t.EventId);
            b.HasOne(t => t.Event).WithMany().HasForeignKey(t => t.EventId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<TournamentParticipant>(b => {
            b.HasKey(p => p.Id);
            b.HasIndex(p => new { p.TournamentId, p.UserId }).IsUnique();
            b.HasOne(p => p.Tournament).WithMany(t => t.Participants).HasForeignKey(p => p.TournamentId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<TournamentMatch>(b => {
            b.HasKey(m => m.Id);
            b.HasIndex(m => new { m.TournamentId, m.Round, m.MatchNumber });
            b.HasOne(m => m.Tournament).WithMany(t => t.Matches).HasForeignKey(m => m.TournamentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Equipment>(e => {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.QrCode).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.QrCode).IsUnique();
        });

        modelBuilder.Entity<EquipmentLoan>(e => {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Equipment).WithMany(eq => eq.Loans).HasForeignKey(x => x.EquipmentId);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId);
        });

    }
}
