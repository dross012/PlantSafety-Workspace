// ============================================================================
//  File:           AlertAutoClosePolicy.cs
//  Created:        2026-05-26
//  Description:
//      Domain policy defining when alerts auto-close. Used by background
//      schedulers and workflow engines. Pure domain logic.
// ============================================================================

using PlantSafety.Domain.Operational.Alerts.Aggregates;
using PlantSafety.Domain.Operational.Alerts.Enums;
using PlantSafety.SharedKernel.Time;

namespace PlantSafety.Domain.Operational.Alerts.Policies;

public sealed class AlertAutoClosePolicy
{
    public bool ShouldAutoClose(Alert alert, UtcTimestamp now)
    {
        if (alert.Status != AlertStatus.Resolved)
            return false;

        // Auto-close after 24 hours unless severity is high
        var threshold = alert.Severity.Code switch
        {
            "Emergency" => TimeSpan.FromHours(1),
            "Critical" => TimeSpan.FromHours(2),
            "High" => TimeSpan.FromHours(4),
            _ => TimeSpan.FromHours(24)
        };

        return alert.ResolvedAtUtc is not null &&
               now.Value >= alert.ResolvedAtUtc.Value.Value.Add(threshold);
    }
}
