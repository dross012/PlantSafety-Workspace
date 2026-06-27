// ============================================================================
//  File:           AlertSourceType.cs
//  Created:        2026-05-26
//  Description:
//      Smart enum representing the source of an alert. Used for routing,
//      verification rules, and analytics.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Enums;

public sealed class AlertSourceType
{
    public string Code { get; }
    public string Name { get; }

    private AlertSourceType(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public static readonly AlertSourceType Human = new("Human", "Human Input");
    public static readonly AlertSourceType Sensor = new("Sensor", "Sensor / IoT");
    public static readonly AlertSourceType Workflow = new("Workflow", "Workflow Engine");
    public static readonly AlertSourceType External = new("External", "External System");

    public static IEnumerable<AlertSourceType> All
        => new[] { Human, Sensor, Workflow, External };

    public bool IsValid() => All.Any(x => x.Code == Code);

    public override string ToString() => Code;
}
