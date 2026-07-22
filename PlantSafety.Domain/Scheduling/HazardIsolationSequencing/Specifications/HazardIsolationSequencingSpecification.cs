// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           HazardIsolationSequencingSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for HazardIsolationSequencing aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Specifications;

/// <summary>Abstract base specification for <see cref="HazardIsolationSequencing"/> aggregate predicate expressions.</summary>
public abstract class HazardIsolationSequencingSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public HazardIsolationSequencingSpecification And(HazardIsolationSequencingSpecification other) => new AndHazardIsolationSequencingSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public HazardIsolationSequencingSpecification Or(HazardIsolationSequencingSpecification other) => new OrHazardIsolationSequencingSpecification(this, other);
}

internal sealed class AndHazardIsolationSequencingSpecification(HazardIsolationSequencingSpecification left, HazardIsolationSequencingSpecification right) : HazardIsolationSequencingSpecification
{
    public override Expression<Func<PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing, bool>>(body, param);
    }
}

internal sealed class OrHazardIsolationSequencingSpecification(HazardIsolationSequencingSpecification left, HazardIsolationSequencingSpecification right) : HazardIsolationSequencingSpecification
{
    public override Expression<Func<PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Aggregates.HazardIsolationSequencing, bool>>(body, param);
    }
}