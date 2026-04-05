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

        SeedRoles(modelBuilder);
    }

    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid>
            {
                Id = new Guid("10000000-0000-0000-0000-000000000001"),
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "10000000-0000-0000-0000-000000000001"
            },
            new IdentityRole<Guid>
            {
                Id = new Guid("10000000-0000-0000-0000-000000000002"),
                Name = "Organizer",
                NormalizedName = "ORGANIZER",
                ConcurrencyStamp = "10000000-0000-0000-0000-000000000002"
            },
            new IdentityRole<Guid>
            {
                Id = new Guid("10000000-0000-0000-0000-000000000003"),
                Name = "Attendee",
                NormalizedName = "ATTENDEE",
                ConcurrencyStamp = "10000000-0000-0000-0000-000000000003"
            },
            new IdentityRole<Guid>
            {
                Id = new Guid("10000000-0000-0000-0000-000000000004"),
                Name = "Operator",
                NormalizedName = "OPERATOR",
                ConcurrencyStamp = "10000000-0000-0000-0000-000000000004"
            }
        );
    }
}
