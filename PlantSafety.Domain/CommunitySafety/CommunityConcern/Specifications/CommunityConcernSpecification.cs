// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcernSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for CommunityConcern aggregate predicate
//                  expressions. Additive-only evolution.
// ============================================================================
using Agg = PlantSafety.Domain.CommunitySafety.CommunityConcern.Aggregates.CommunityConcern;

namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Specifications;

/// <summary>Abstract base specification for <see cref="Agg"/> aggregate predicate expressions.</summary>
public abstract class CommunityConcernSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Agg, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Agg, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public CommunityConcernSpecification And(CommunityConcernSpecification other) => new AndCommunityConcernSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public CommunityConcernSpecification Or(CommunityConcernSpecification other) => new OrCommunityConcernSpecification(this, other);
}

internal sealed class AndCommunityConcernSpecification(CommunityConcernSpecification left, CommunityConcernSpecification right) : CommunityConcernSpecification
{
    public override Expression<Func<Agg, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Agg, bool>>(body, param);
    }
}

internal sealed class OrCommunityConcernSpecification(CommunityConcernSpecification left, CommunityConcernSpecification right) : CommunityConcernSpecification
{
    public override Expression<Func<Agg, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Agg, bool>>(body, param);
    }
}

/// <summary>Filters CommunityConcern aggregates to those matching a given lifecycle status.</summary>
public sealed class CommunityConcernByStatusSpecification(Enums.CommunityConcernStatus status) : CommunityConcernSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<Agg, bool>> ToExpression() => agg => agg.Status == status;
}

/// <summary>Filters CommunityConcern aggregates to those still open (not Resolved/Archived).</summary>
public sealed class OpenCommunityConcernsSpecification : CommunityConcernSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<Agg, bool>> ToExpression()
        => agg => agg.Status != Enums.CommunityConcernStatus.Resolved && agg.Status != Enums.CommunityConcernStatus.Archived;
}
