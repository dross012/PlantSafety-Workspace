// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for Dashboard aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Dashboard.Specifications;

/// <summary>Abstract base specification for <see cref="Aggregates.Dashboard"/> aggregate predicate expressions.</summary>
public abstract class DashboardSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Aggregates.Dashboard, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Aggregates.Dashboard, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public DashboardSpecification And(DashboardSpecification other) => new AndDashboardSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public DashboardSpecification Or(DashboardSpecification other) => new OrDashboardSpecification(this, other);
}

internal sealed class AndDashboardSpecification(DashboardSpecification left, DashboardSpecification right) : DashboardSpecification
{
    public override Expression<Func<Aggregates.Dashboard, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.Dashboard, bool>>(body, param);
    }
}

internal sealed class OrDashboardSpecification(DashboardSpecification left, DashboardSpecification right) : DashboardSpecification
{
    public override Expression<Func<Aggregates.Dashboard, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.Dashboard, bool>>(body, param);
    }
}
