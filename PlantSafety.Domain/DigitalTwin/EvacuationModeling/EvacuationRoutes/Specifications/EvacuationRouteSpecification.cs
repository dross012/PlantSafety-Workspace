// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           EvacuationRouteSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for EvacuationRoute aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Specifications;

/// <summary>Abstract base specification for <see cref="Aggregates.EvacuationRoute"/> aggregate predicate expressions.</summary>
public abstract class EvacuationRouteSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Aggregates.EvacuationRoute, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Aggregates.EvacuationRoute, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public EvacuationRouteSpecification And(EvacuationRouteSpecification other) => new AndEvacuationRouteSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public EvacuationRouteSpecification Or(EvacuationRouteSpecification other) => new OrEvacuationRouteSpecification(this, other);
}

internal sealed class AndEvacuationRouteSpecification(EvacuationRouteSpecification left, EvacuationRouteSpecification right) : EvacuationRouteSpecification
{
    public override Expression<Func<Aggregates.EvacuationRoute, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.EvacuationRoute, bool>>(body, param);
    }
}

internal sealed class OrEvacuationRouteSpecification(EvacuationRouteSpecification left, EvacuationRouteSpecification right) : EvacuationRouteSpecification
{
    public override Expression<Func<Aggregates.EvacuationRoute, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.EvacuationRoute, bool>>(body, param);
    }
}

/// <summary>Matches evacuation routes belonging to a specific zone.</summary>
public sealed class ByZoneEvacuationRouteSpecification(EvacuationZoneId zoneId) : EvacuationRouteSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<Aggregates.EvacuationRoute, bool>> ToExpression()
        => route => route.ZoneId.Equals(zoneId);
}

/// <summary>Matches evacuation routes currently carrying at least one open obstruction.</summary>
public sealed class ActivelyObstructedEvacuationRouteSpecification : EvacuationRouteSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<Aggregates.EvacuationRoute, bool>> ToExpression()
        => route => route.Status == EvacuationRouteStatus.Obstructed;
}
