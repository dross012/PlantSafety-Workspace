// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Domain
//  File:           PPEZoneMappingSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PPEZoneMapping aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEZoneMapping.Specifications;

/// <summary>Abstract base specification for <see cref="PPEZoneMapping"/> aggregate predicate expressions.</summary>
public abstract class PPEZoneMappingSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PPEZoneMappingSpecification And(PPEZoneMappingSpecification other) => new AndPPEZoneMappingSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PPEZoneMappingSpecification Or(PPEZoneMappingSpecification other) => new OrPPEZoneMappingSpecification(this, other);
}

internal sealed class AndPPEZoneMappingSpecification(PPEZoneMappingSpecification left, PPEZoneMappingSpecification right) : PPEZoneMappingSpecification
{
    public override Expression<Func<PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping, bool>>(body, param);
    }
}

internal sealed class OrPPEZoneMappingSpecification(PPEZoneMappingSpecification left, PPEZoneMappingSpecification right) : PPEZoneMappingSpecification
{
    public override Expression<Func<PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PPE.PPEZoneMapping.Aggregates.PPEZoneMapping, bool>>(body, param);
    }
}