// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           PMSchedulingSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PMScheduling aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Specifications;

/// <summary>Abstract base specification for <see cref="PMScheduling"/> aggregate predicate expressions.</summary>
public abstract class PMSchedulingSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PMSchedulingSpecification And(PMSchedulingSpecification other) => new AndPMSchedulingSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PMSchedulingSpecification Or(PMSchedulingSpecification other) => new OrPMSchedulingSpecification(this, other);
}

internal sealed class AndPMSchedulingSpecification(PMSchedulingSpecification left, PMSchedulingSpecification right) : PMSchedulingSpecification
{
    public override Expression<Func<PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling, bool>>(body, param);
    }
}

internal sealed class OrPMSchedulingSpecification(PMSchedulingSpecification left, PMSchedulingSpecification right) : PMSchedulingSpecification
{
    public override Expression<Func<PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Scheduling.PMScheduling.Aggregates.PMScheduling, bool>>(body, param);
    }
}