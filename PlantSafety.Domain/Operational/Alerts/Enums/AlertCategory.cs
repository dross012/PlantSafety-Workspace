// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Alerts
//  Layer:          Domain
//  File:           AlertCategory.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Smart enum representing the canonical alert categories used across
//      the Alerts module. Categories are stable for analytics and routing.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Enums;

public sealed class AlertCategory
{
    public string Code { get; }
    public string Name { get; }

    private AlertCategory(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public static readonly AlertCategory Safety = new("Safety", "Safety");
    public static readonly AlertCategory Environmental = new("Environmental", "Environmental");
    public static readonly AlertCategory Equipment = new("Equipment", "Equipment");
    public static readonly AlertCategory Compliance = new("Compliance", "Compliance");

    public static IEnumerable<AlertCategory> All
        => new[] { Safety, Environmental, Equipment, Compliance };

    public bool IsValid() => All.Any(x => x.Code == Code);

    public override string ToString() => Code;
}
