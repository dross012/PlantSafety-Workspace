// ============================================================================
//  File:           AlertSourcePolicy.cs
//  Created:        2026-05-26
//  Description:
//      Domain policy defining allowed alert sources and source-specific rules.
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Policies;

public sealed class AlertSourcePolicy
{
    public bool IsValidSource(AlertSourceType source)
        => source.Code is "Human" or "Sensor" or "Workflow" or "External";

    public bool RequiresVerification(AlertSourceType source)
        => source.Code is "External" or "Sensor";
}
