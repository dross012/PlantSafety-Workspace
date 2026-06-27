// ============================================================================
//  File:           AlertsRequiringInvestigationSpecification.cs
//  Created:        2026-05-26
//  Description:
//      Alerts that are High/Critical/Emergency AND not yet InProgress.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Specifications;

public sealed class AlertsRequiringInvestigationSpecification : AlertSpecification
{
    public override Expression<Func<Alert, bool>> ToExpression()
        => alert =>
            (alert.Severity == AlertSeverity.High ||
             alert.Severity == AlertSeverity.Critical ||
             alert.Severity == AlertSeverity.Emergency)
            &&
            (alert.Status == AlertStatus.Created ||
             alert.Status == AlertStatus.Acknowledged);
}
