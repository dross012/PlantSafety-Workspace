// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Domain
//  File:           AccumulationAreaSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Abstract base specification for AccumulationArea aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Aggregates;

namespace PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Specifications;

/// <summary>Abstract base specification for AccumulationArea aggregate predicate expressions.</summary>
public abstract class AccumulationAreaSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<AccumulationArea, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<AccumulationArea, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public AccumulationAreaSpecification And(AccumulationAreaSpecification other) => new AndAccumulationAreaSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public AccumulationAreaSpecification Or(AccumulationAreaSpecification other) => new OrAccumulationAreaSpecification(this, other);
}

internal sealed class AndAccumulationAreaSpecification(AccumulationAreaSpecification left, AccumulationAreaSpecification right) : AccumulationAreaSpecification
{
    public override Expression<Func<AccumulationArea, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<AccumulationArea, bool>>(body, param);
    }
}

internal sealed class OrAccumulationAreaSpecification(AccumulationAreaSpecification left, AccumulationAreaSpecification right) : AccumulationAreaSpecification
{
    public override Expression<Func<AccumulationArea, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<AccumulationArea, bool>>(body, param);
    }
}
