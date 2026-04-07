namespace LanManager.Maui.Shared.Services;

/// <summary>
/// Singleton that holds the currently selected event so every page can
/// access it without relying on query-string parameters.
/// </summary>
public class AppStateService
{
    public Guid EventId { get; private set; }
    public string EventName { get; private set; } = string.Empty;
    public bool HasEvent => EventId != Guid.Empty;

    public void SetEvent(Guid id, string name)
    {
        EventId = id;
        EventName = name;
    }

    public void Clear() => (EventId, EventName) = (Guid.Empty, string.Empty);
}
