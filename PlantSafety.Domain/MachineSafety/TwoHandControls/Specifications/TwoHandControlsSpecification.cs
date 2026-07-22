// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.TwoHandControls
//  Layer:          Domain
//  File:           TwoHandControlsSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for TwoHandControls aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.TwoHandControls.Specifications;

/// <summary>Abstract base specification for <see cref="TwoHandControls"/> aggregate predicate expressions.</summary>
public abstract class TwoHandControlsSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public TwoHandControlsSpecification And(TwoHandControlsSpecification other) => new AndTwoHandControlsSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public TwoHandControlsSpecification Or(TwoHandControlsSpecification other) => new OrTwoHandControlsSpecification(this, other);
}

internal sealed class AndTwoHandControlsSpecification(TwoHandControlsSpecification left, TwoHandControlsSpecification right) : TwoHandControlsSpecification
{
    public override Expression<Func<PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls, bool>>(body, param);
    }
}

internal sealed class OrTwoHandControlsSpecification(TwoHandControlsSpecification left, TwoHandControlsSpecification right) : TwoHandControlsSpecification
{
    public override Expression<Func<PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.MachineSafety.TwoHandControls.Aggregates.TwoHandControls, bool>>(body, param);
    }
}