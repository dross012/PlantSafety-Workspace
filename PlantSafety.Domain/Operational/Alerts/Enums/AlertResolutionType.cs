// ============================================================================
//  File:           AlertResolutionType.cs
//  Created:        2026-05-26
//  Description:
//      Smart enum representing how an alert was resolved. Required for
//      analytics, reporting, and compliance.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Enums;

public sealed class AlertResolutionType
{
    public string Code { get; }
    public string Name { get; }

    private AlertResolutionType(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public static readonly AlertResolutionType Fixed = new("Fixed", "Issue Fixed");
    public static readonly AlertResolutionType Mitigated = new("Mitigated", "Risk Mitigated");
    public static readonly AlertResolutionType FalseAlarm = new("FalseAlarm", "False Alarm");
    public static readonly AlertResolutionType NoActionNeeded = new("NoActionNeeded", "No Action Required");
    public static readonly AlertResolutionType External = new("External", "Resolved Externally");

    public static IEnumerable<AlertResolutionType> All
        => new[] { Fixed, Mitigated, FalseAlarm, NoActionNeeded, External };

    public bool IsValid() => All.Any(x => x.Code == Code);

    public override string ToString() => Code;
}
