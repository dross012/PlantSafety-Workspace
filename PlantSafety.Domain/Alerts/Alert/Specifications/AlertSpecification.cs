// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for Alert aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Alerts.Alert.Specifications;

/// <summary>Abstract base specification for <see cref="Aggregates.Alert"/> aggregate predicate expressions.</summary>
public abstract class AlertSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Aggregates.Alert, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Aggregates.Alert, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public AlertSpecification And(AlertSpecification other) => new AndAlertSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public AlertSpecification Or(AlertSpecification other) => new OrAlertSpecification(this, other);
}

internal sealed class AndAlertSpecification(AlertSpecification left, AlertSpecification right) : AlertSpecification
{
    public override Expression<Func<Aggregates.Alert, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.Alert, bool>>(body, param);
    }
}

internal sealed class OrAlertSpecification(AlertSpecification left, AlertSpecification right) : AlertSpecification
{
    public override Expression<Func<Aggregates.Alert, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.Alert, bool>>(body, param);
    }
}
