// ============================================================================
//  File:           AlertSeverityRules.cs
//  Created:        2026-05-26
//  Description:
//      Severity-based domain rules for Alerts. Defines escalation urgency,
//      routing priority, and SLA defaults.
// ============================================================================


// ============================================================================
//  File:           AlertSeverityRules.cs
//  Created:        2026-05-26
//  Description:
//      Severity-based domain rules for Alerts. Defines escalation urgency,
//      routing priority, and SLA defaults.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Rules;

public static class AlertSeverityRules
{
    public static int GetRoutingPriority(AlertSeverity severity) =>
        severity.Code switch
        {
            "Emergency" => 1,
            "Critical" => 2,
            "High" => 3,
            "Medium" => 4,
            "Low" => 5,
            _ => 5
        };

    public static bool RequiresImmediateEscalation(AlertSeverity severity) =>
        severity.Code is "Emergency" or "Critical";

    public static TimeSpan GetDefaultSla(AlertSeverity severity) =>
        severity.Code switch
        {
            "Emergency" => TimeSpan.FromMinutes(5),
            "Critical" => TimeSpan.FromMinutes(15),
            "High" => TimeSpan.FromHours(1),
            "Medium" => TimeSpan.FromHours(4),
            "Low" => TimeSpan.FromHours(24),
            _ => TimeSpan.FromHours(24)
        };
}
