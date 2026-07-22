// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           WidgetSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for Widget aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Widget.Specifications;

/// <summary>Abstract base specification for <see cref="Aggregates.Widget"/> aggregate predicate expressions.</summary>
public abstract class WidgetSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Aggregates.Widget, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Aggregates.Widget, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public WidgetSpecification And(WidgetSpecification other) => new AndWidgetSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public WidgetSpecification Or(WidgetSpecification other) => new OrWidgetSpecification(this, other);
}

internal sealed class AndWidgetSpecification(WidgetSpecification left, WidgetSpecification right) : WidgetSpecification
{
    public override Expression<Func<Aggregates.Widget, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.Widget, bool>>(body, param);
    }
}

internal sealed class OrWidgetSpecification(WidgetSpecification left, WidgetSpecification right) : WidgetSpecification
{
    public override Expression<Func<Aggregates.Widget, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.Widget, bool>>(body, param);
    }
}
