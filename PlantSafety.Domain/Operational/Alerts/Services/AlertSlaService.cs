// ============================================================================
//  File:           AlertSlaService.cs
//  Created:        2026-05-26
//  Description:
//      Domain service for SLA logic. Determines expiration, breach, and
//      auto-escalation triggers. No infrastructure logic.
// ============================================================================

using PlantSafety.Domain.Operational.Alerts.Aggregates;
using PlantSafety.Domain.Operational.Alerts.Rules;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Domain.Operational.Alerts.Services;

public sealed class AlertSlaService
{
    public static bool IsBreached(Alert alert, UtcTimestamp now)
        => alert.SlaExpiration is not null &&
           AlertSlaRules.IsSlaBreached(now, alert.SlaExpiration.Value);

    public static bool ShouldAutoEscalate(Alert alert, UtcTimestamp now)
        => alert.SlaExpiration is not null &&
           AlertSlaRules.ShouldAutoEscalate(
               alert.Severity,
               now,
               alert.SlaExpiration.Value);
}
