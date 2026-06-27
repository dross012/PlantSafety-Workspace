// ============================================================================
//  File:           UnacknowledgedAlertsSpecification.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Specifications;

public sealed class UnacknowledgedAlertsSpecification : AlertSpecification
{
    public override Expression<Func<Alert, bool>> ToExpression()
        => alert => alert.Status == AlertStatus.Created;
}
