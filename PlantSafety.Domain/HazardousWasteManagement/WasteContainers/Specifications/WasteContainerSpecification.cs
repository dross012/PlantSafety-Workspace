// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Domain
//  File:           WasteContainerSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Abstract base specification for WasteContainer aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Aggregates;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Specifications;

/// <summary>Abstract base specification for WasteContainer aggregate predicate expressions.</summary>
public abstract class WasteContainerSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<WasteContainer, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<WasteContainer, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public WasteContainerSpecification And(WasteContainerSpecification other) => new AndWasteContainerSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public WasteContainerSpecification Or(WasteContainerSpecification other) => new OrWasteContainerSpecification(this, other);
}

internal sealed class AndWasteContainerSpecification(WasteContainerSpecification left, WasteContainerSpecification right) : WasteContainerSpecification
{
    public override Expression<Func<WasteContainer, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<WasteContainer, bool>>(body, param);
    }
}

internal sealed class OrWasteContainerSpecification(WasteContainerSpecification left, WasteContainerSpecification right) : WasteContainerSpecification
{
    public override Expression<Func<WasteContainer, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<WasteContainer, bool>>(body, param);
    }
}
