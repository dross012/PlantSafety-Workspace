// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Domain
//  File:           EvacuationBottleneckSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for EvacuationBottleneck aggregate predicate
//                  expressions. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Specifications;

/// <summary>Abstract base specification for <see cref="Aggregates.EvacuationBottleneck"/> aggregate predicate expressions.</summary>
public abstract class EvacuationBottleneckSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Aggregates.EvacuationBottleneck, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Aggregates.EvacuationBottleneck, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public EvacuationBottleneckSpecification And(EvacuationBottleneckSpecification other) => new AndEvacuationBottleneckSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public EvacuationBottleneckSpecification Or(EvacuationBottleneckSpecification other) => new OrEvacuationBottleneckSpecification(this, other);
}

internal sealed class AndEvacuationBottleneckSpecification(EvacuationBottleneckSpecification left, EvacuationBottleneckSpecification right) : EvacuationBottleneckSpecification
{
    public override Expression<Func<Aggregates.EvacuationBottleneck, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.EvacuationBottleneck, bool>>(body, param);
    }
}

internal sealed class OrEvacuationBottleneckSpecification(EvacuationBottleneckSpecification left, EvacuationBottleneckSpecification right) : EvacuationBottleneckSpecification
{
    public override Expression<Func<Aggregates.EvacuationBottleneck, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.EvacuationBottleneck, bool>>(body, param);
    }
}

/// <summary>Matches bottleneck history records for a specific route.</summary>
public sealed class ByRouteEvacuationBottleneckSpecification(EvacuationRouteId routeId) : EvacuationBottleneckSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<Aggregates.EvacuationBottleneck, bool>> ToExpression()
        => bottleneck => bottleneck.RouteId.Equals(routeId);
}
