// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           WasteStreamSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Abstract base specification for WasteStream aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Aggregates;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Specifications;

/// <summary>Abstract base specification for WasteStream aggregate predicate expressions.</summary>
public abstract class WasteStreamSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<WasteStream, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<WasteStream, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public WasteStreamSpecification And(WasteStreamSpecification other) => new AndWasteStreamSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public WasteStreamSpecification Or(WasteStreamSpecification other) => new OrWasteStreamSpecification(this, other);
}

internal sealed class AndWasteStreamSpecification(WasteStreamSpecification left, WasteStreamSpecification right) : WasteStreamSpecification
{
    public override Expression<Func<WasteStream, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<WasteStream, bool>>(body, param);
    }
}

internal sealed class OrWasteStreamSpecification(WasteStreamSpecification left, WasteStreamSpecification right) : WasteStreamSpecification
{
    public override Expression<Func<WasteStream, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<WasteStream, bool>>(body, param);
    }
}
