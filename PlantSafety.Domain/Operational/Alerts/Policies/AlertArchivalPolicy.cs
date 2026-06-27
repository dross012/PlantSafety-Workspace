// ============================================================================
//  File:           AlertArchivalPolicy.cs
//  Created:        2026-05-26
//  Description:
//      Domain policy defining archival rules. Alerts are archived after a
//      retention period following closure.
// ============================================================================

using PlantSafety.Operational.Alerts.Domain.Alerts.Aggregates;
using PlantSafety.Operational.Alerts.Domain.Alerts.Enums;
using PlantSafety.SharedKernel.Time;

namespace PlantSafety.Domain.Operational.Alerts.Policies;

public sealed class AlertArchivalPolicy
{
    public bool ShouldArchive(Alert alert, UtcTimestamp now)
    {
        if (alert.Status != AlertStatus.Closed)
            return false;

        // Retention varies by severity
        var retention = alert.Severity.Code switch
        {
            "Emergency" => TimeSpan.FromDays(90),
            "Critical" => TimeSpan.FromDays(60),
            "High" => TimeSpan.FromDays(30),
            "Medium" => TimeSpan.FromDays(14),
            _ => TimeSpan.FromDays(7)
        };

        return alert.ClosedAtUtc is not null &&
               now.Value >= alert.ClosedAtUtc.Value.Value.Add(retention);
    }
}
