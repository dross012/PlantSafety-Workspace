// ============================================================================
//  File:           OverdueAlertsSpecification.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Specifications;

public sealed class OverdueAlertsSpecification : AlertSpecification
{
    private readonly UtcTimestamp _now;

    public OverdueAlertsSpecification(UtcTimestamp now)
        => _now = now;

    public override Expression<Func<Alert, bool>> ToExpression()
        => alert =>
            alert.SlaExpiration != null &&
            _now.Value >= alert.SlaExpiration.Value.Value;
}
