// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Domain
//  File:           DashboardSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Specifications;

/// <summary>Abstract base specification for <see cref="Dashboard"/> aggregate predicate expressions.</summary>
public abstract class DashboardSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Dashboard, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Dashboard, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public DashboardSpecification And(DashboardSpecification other) => new AndDashboardSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public DashboardSpecification Or(DashboardSpecification other) => new OrDashboardSpecification(this, other);
}

internal sealed class AndDashboardSpecification(DashboardSpecification left, DashboardSpecification right) : DashboardSpecification
{
    public override Expression<Func<Dashboard, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Dashboard, bool>>(body, param);
    }
}

internal sealed class OrDashboardSpecification(DashboardSpecification left, DashboardSpecification right) : DashboardSpecification
{
    public override Expression<Func<Dashboard, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Dashboard, bool>>(body, param);
    }
}
