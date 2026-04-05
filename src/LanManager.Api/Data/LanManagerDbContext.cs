using LanManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LanManager.Api.Data;

public class LanManagerDbContext(DbContextOptions<LanManagerDbContext> options) : DbContext(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<CheckInRecord> CheckInRecords => Set<CheckInRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
    }
}
