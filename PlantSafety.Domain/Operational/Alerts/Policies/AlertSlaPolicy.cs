// ============================================================================
//  File:           AlertSlaPolicy.cs
//  Created:        2026-05-26
//  Description:
//      Domain policy defining SLA expiration logic. Pure domain logic.
// ============================================================================

using PlantSafety.Operational.Alerts.Domain.Alerts.Aggregates;
using PlantSafety.Operational.Alerts.Domain.Alerts.Enums;
using PlantSafety.SharedKernel.Time;

namespace PlantSafety.Domain.Operational.Alerts.Policies;

public sealed class AlertSlaPolicy
{
    public UtcTimestamp CalculateExpiration(Alert alert)
    {
        var duration = alert.Severity.Code switch
        {
            "Emergency" => TimeSpan.FromMinutes(5),
            "Critical" => TimeSpan.FromMinutes(15),
            "High" => TimeSpan.FromHours(1),
            "Medium" => TimeSpan.FromHours(4),
            _ => TimeSpan.FromHours(24)
        };

        return new UtcTimestamp(alert.CreatedAtUtc.Value.Add(duration));
    }
}
