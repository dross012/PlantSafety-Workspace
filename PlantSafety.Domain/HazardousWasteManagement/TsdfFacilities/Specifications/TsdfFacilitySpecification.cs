// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.TsdfFacilities
//  Layer:          Domain
//  File:           TsdfFacilitySpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Abstract base specification for TsdfFacility aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Aggregates;

namespace PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Specifications;

/// <summary>Abstract base specification for TsdfFacility aggregate predicate expressions.</summary>
public abstract class TsdfFacilitySpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<TsdfFacility, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<TsdfFacility, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public TsdfFacilitySpecification And(TsdfFacilitySpecification other) => new AndTsdfFacilitySpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public TsdfFacilitySpecification Or(TsdfFacilitySpecification other) => new OrTsdfFacilitySpecification(this, other);
}

internal sealed class AndTsdfFacilitySpecification(TsdfFacilitySpecification left, TsdfFacilitySpecification right) : TsdfFacilitySpecification
{
    public override Expression<Func<TsdfFacility, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<TsdfFacility, bool>>(body, param);
    }
}

internal sealed class OrTsdfFacilitySpecification(TsdfFacilitySpecification left, TsdfFacilitySpecification right) : TsdfFacilitySpecification
{
    public override Expression<Func<TsdfFacility, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<TsdfFacility, bool>>(body, param);
    }
}
