// ============================================================================
//  File:           AlertLifecycleStage.cs
//  Created:        2026-05-26
//  Description:
//      Smart enum representing high-level lifecycle stages used for dashboards,
//      analytics, and SLA tracking. This is NOT the same as AlertStatus.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Enums;

public sealed class AlertLifecycleStage
{
    public string Code { get; }
    public string Name { get; }

    private AlertLifecycleStage(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public static readonly AlertLifecycleStage New = new("New", "New");
    public static readonly AlertLifecycleStage Active = new("Active", "Active");
    public static readonly AlertLifecycleStage Escalated = new("Escalated", "Escalated");
    public static readonly AlertLifecycleStage Resolved = new("Resolved", "Resolved");
    public static readonly AlertLifecycleStage Closed = new("Closed", "Closed");
    public static readonly AlertLifecycleStage Archived = new("Archived", "Archived");

    public static IEnumerable<AlertLifecycleStage> All
        => new[] { New, Active, Escalated, Resolved, Closed, Archived };

    public bool IsValid() => All.Any(x => x.Code == Code);

    public override string ToString() => Code;
}
