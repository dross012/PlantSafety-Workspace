// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           EvacuationZoneSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for EvacuationZone aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Specifications;

/// <summary>Abstract base specification for <see cref="Aggregates.EvacuationZone"/> aggregate predicate expressions.</summary>
public abstract class EvacuationZoneSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Aggregates.EvacuationZone, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Aggregates.EvacuationZone, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public EvacuationZoneSpecification And(EvacuationZoneSpecification other) => new AndEvacuationZoneSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public EvacuationZoneSpecification Or(EvacuationZoneSpecification other) => new OrEvacuationZoneSpecification(this, other);
}

internal sealed class AndEvacuationZoneSpecification(EvacuationZoneSpecification left, EvacuationZoneSpecification right) : EvacuationZoneSpecification
{
    public override Expression<Func<Aggregates.EvacuationZone, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.EvacuationZone, bool>>(body, param);
    }
}

internal sealed class OrEvacuationZoneSpecification(EvacuationZoneSpecification left, EvacuationZoneSpecification right) : EvacuationZoneSpecification
{
    public override Expression<Func<Aggregates.EvacuationZone, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Aggregates.EvacuationZone, bool>>(body, param);
    }
}
