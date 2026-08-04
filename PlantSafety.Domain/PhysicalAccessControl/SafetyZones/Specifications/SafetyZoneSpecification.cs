// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Domain
//  File:           SafetyZoneSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for SafetyZone aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Specifications;

/// <summary>Abstract base specification for <see cref="SafetyZone"/> aggregate predicate expressions.</summary>
public abstract class SafetyZoneSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public SafetyZoneSpecification And(SafetyZoneSpecification other) => new AndSafetyZoneSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public SafetyZoneSpecification Or(SafetyZoneSpecification other) => new OrSafetyZoneSpecification(this, other);
}

internal sealed class AndSafetyZoneSpecification(SafetyZoneSpecification left, SafetyZoneSpecification right) : SafetyZoneSpecification
{
    public override Expression<Func<PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone, bool>>(body, param);
    }
}

internal sealed class OrSafetyZoneSpecification(SafetyZoneSpecification left, SafetyZoneSpecification right) : SafetyZoneSpecification
{
    public override Expression<Func<PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates.SafetyZone, bool>>(body, param);
    }
}
