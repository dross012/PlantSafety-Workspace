// ============================================================================
//  File:           AlertsBySeveritySpecification.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Specifications;

public sealed class AlertsBySeveritySpecification : AlertSpecification
{
    private readonly AlertSeverity _severity;

    public AlertsBySeveritySpecification(AlertSeverity severity)
        => _severity = severity;

    public override Expression<Func<Alert, bool>> ToExpression()
        => alert => alert.Severity == _severity;
}
