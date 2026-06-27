// ============================================================================
//  File:           AlertPriority.cs
//  Created:        2026-05-26
//  Description:
//      Smart enum representing routing priority. Derived from severity but
//      explicitly modeled for workflow engines and dashboards.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Enums;

public sealed class AlertPriority
{
    public int Level { get; }
    public string Name { get; }

    private AlertPriority(int level, string name)
    {
        Level = level;
        Name = name;
    }

    public static readonly AlertPriority P1 = new(1, "P1 - Immediate");
    public static readonly AlertPriority P2 = new(2, "P2 - High");
    public static readonly AlertPriority P3 = new(3, "P3 - Medium");
    public static readonly AlertPriority P4 = new(4, "P4 - Low");
    public static readonly AlertPriority P5 = new(5, "P5 - Deferred");

    public static IEnumerable<AlertPriority> All
        => new[] { P1, P2, P3, P4, P5 };

    public bool IsValid() => All.Any(x => x.Level == Level);

    public override string ToString() => Name;
}
