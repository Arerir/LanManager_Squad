using Microsoft.AspNetCore.Identity;

namespace LanManager.Api.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Registration> Registrations { get; set; } = [];
    public ICollection<CheckInRecord> CheckInRecords { get; set; } = [];
}
