// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitScheduling
//  Layer:          Domain
//  File:           PermitSchedulingSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PermitScheduling aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitScheduling.Specifications;

/// <summary>Abstract base specification for <see cref="PermitScheduling"/> aggregate predicate expressions.</summary>
public abstract class PermitSchedulingSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PermitSchedulingSpecification And(PermitSchedulingSpecification other) => new AndPermitSchedulingSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PermitSchedulingSpecification Or(PermitSchedulingSpecification other) => new OrPermitSchedulingSpecification(this, other);
}

internal sealed class AndPermitSchedulingSpecification(PermitSchedulingSpecification left, PermitSchedulingSpecification right) : PermitSchedulingSpecification
{
    public override Expression<Func<PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling, bool>>(body, param);
    }
}

internal sealed class OrPermitSchedulingSpecification(PermitSchedulingSpecification left, PermitSchedulingSpecification right) : PermitSchedulingSpecification
{
    public override Expression<Func<PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PermitToWork.PermitScheduling.Aggregates.PermitScheduling, bool>>(body, param);
    }
}