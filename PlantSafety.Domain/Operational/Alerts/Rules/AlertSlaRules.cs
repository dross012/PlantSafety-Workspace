// ============================================================================
//  File:           AlertSlaRules.cs
//  Created:        2026-05-26
//  Description:
//      SLA rules for Alerts. Determines expiration, breach detection, and
//      escalation triggers. Complies with SLA rules in the Alerts Playbook.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Rules;

public static class AlertSlaRules
{
    public static bool IsSlaBreached(UtcTimestamp now, UtcTimestamp slaExpiration)
        => now.Value >= slaExpiration.Value;

    public static bool ShouldAutoEscalate(AlertSeverity severity, UtcTimestamp now, UtcTimestamp slaExpiration)
        => IsSlaBreached(now, slaExpiration) &&
           AlertSeverityRules.RequiresImmediateEscalation(severity);
}
