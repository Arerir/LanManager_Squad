namespace LanManager.Api.Models;

[Flags]
public enum ReportSections
{
    Summary       = 1,
    Registrations = 2,
    CheckIns      = 4,
    Equipment     = 8,
    Tournaments   = 16,
    All           = Summary | Registrations | CheckIns | Equipment | Tournaments
}
