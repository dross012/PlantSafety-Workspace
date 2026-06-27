// ============================================================================
//  File:           AlertsByAssignedUserSpecification.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Specifications;

public sealed class AlertsByAssignedUserSpecification : AlertSpecification
{
    private readonly string _assignedTo;

    public AlertsByAssignedUserSpecification(string assignedTo)
        => _assignedTo = assignedTo.Trim();

    public override Expression<Func<Alert, bool>> ToExpression()
        => alert => alert.Assignments.Any(a => a.AssignedTo == _assignedTo);
}
