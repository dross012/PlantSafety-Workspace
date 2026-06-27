// ============================================================================
//  File:           OrAlertSpecification.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Specifications;

public sealed class OrAlertSpecification : AlertSpecification
{
    private readonly AlertSpecification _left;
    private readonly AlertSpecification _right;

    public OrAlertSpecification(AlertSpecification left, AlertSpecification right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<Alert, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();

        var param = Expression.Parameter(typeof(Alert), "alert");

        var body = Expression.OrElse(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param));

        return Expression.Lambda<Func<Alert, bool>>(body, param);
    }
}
