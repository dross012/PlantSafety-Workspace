// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           HazmatMaterialSpecification.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Composable predicate specification base for HazmatMaterial query filtering.
//                  Mirrors Compliance.Permits.Specifications.PermitsSpecification.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.Aggregates;

namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Specifications;

/// <summary>Abstract base specification for <c>HazmatMaterial</c> aggregate predicate expressions.</summary>
public abstract class HazmatMaterialSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<HazmatMaterial, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<HazmatMaterial, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public HazmatMaterialSpecification And(HazmatMaterialSpecification other) => new AndHazmatMaterialSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public HazmatMaterialSpecification Or(HazmatMaterialSpecification other) => new OrHazmatMaterialSpecification(this, other);
}

internal sealed class AndHazmatMaterialSpecification(HazmatMaterialSpecification left, HazmatMaterialSpecification right) : HazmatMaterialSpecification
{
    public override Expression<Func<HazmatMaterial, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<HazmatMaterial, bool>>(body, param);
    }
}

internal sealed class OrHazmatMaterialSpecification(HazmatMaterialSpecification left, HazmatMaterialSpecification right) : HazmatMaterialSpecification
{
    public override Expression<Func<HazmatMaterial, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<HazmatMaterial, bool>>(body, param);
    }
}
