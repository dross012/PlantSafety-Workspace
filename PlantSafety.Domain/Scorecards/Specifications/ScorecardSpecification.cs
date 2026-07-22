// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           ScorecardSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for Scorecard aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
using System.Linq.Expressions;
using PlantSafety.Domain.Scorecards.Aggregates;

namespace PlantSafety.Domain.Scorecards.Specifications;

/// <summary>Abstract base specification for Scorecard aggregate predicate expressions.</summary>
public abstract class ScorecardSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Scorecard, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Scorecard, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public ScorecardSpecification And(ScorecardSpecification other) => new AndScorecardSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public ScorecardSpecification Or(ScorecardSpecification other) => new OrScorecardSpecification(this, other);
}

internal sealed class AndScorecardSpecification(ScorecardSpecification left, ScorecardSpecification right) : ScorecardSpecification
{
    public override Expression<Func<Scorecard, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Scorecard, bool>>(body, param);
    }
}

internal sealed class OrScorecardSpecification(ScorecardSpecification left, ScorecardSpecification right) : ScorecardSpecification
{
    public override Expression<Func<Scorecard, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Scorecard, bool>>(body, param);
    }
}