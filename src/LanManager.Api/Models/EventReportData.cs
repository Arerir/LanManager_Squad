using LanManager.Data.Models;

namespace LanManager.Api.Models;

public class EventReportData
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Location { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int Capacity { get; init; }
    public Data.Models.EventStatus Status { get; init; }

    public RegistrationSummary[]? Registrations { get; init; }
    public CheckInSummary[]? CheckIns { get; init; }

    // Placeholders — not yet populated
    public EquipmentLoan[]? Equipment { get; init; }
    public object[]? Tournaments { get; init; }
}

public class RegistrationSummary
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public Data.Models.RegistrationStatus Status { get; init; }
    public DateTime RegisteredAt { get; init; }
}

public class CheckInSummary
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public DateTime CheckedInAt { get; init; }
    public DateTime? CheckedOutAt { get; init; }
    public TimeSpan? Duration { get; init; }
}
