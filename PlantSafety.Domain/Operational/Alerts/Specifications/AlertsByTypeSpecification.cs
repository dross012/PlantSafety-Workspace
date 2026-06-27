// ============================================================================
//  File:           AlertsByTypeSpecification.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Specifications;

public sealed class AlertsByTypeSpecification : AlertSpecification
{
    private readonly AlertType _type;

    public AlertsByTypeSpecification(AlertType type)
        => _type = type;

    public override Expression<Func<Alert, bool>> ToExpression()
        => alert => alert.AlertType == _type;
}
