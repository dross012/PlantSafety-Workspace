// ============================================================================
//  File:           AlertsByStatusSpecification.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Specifications;

public sealed class AlertsByStatusSpecification : AlertSpecification
{
    private readonly AlertStatus _status;

    public AlertsByStatusSpecification(AlertStatus status)
        => _status = status;

    public override Expression<Func<Alert, bool>> ToExpression()
        => alert => alert.Status == _status;
}
