// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for Risk aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
using System.Linq.Expressions;
using PlantSafety.Domain.RiskManagement.Aggregates;

namespace PlantSafety.Domain.RiskManagement.Specifications;

/// <summary>Abstract base specification for Risk aggregate predicate expressions.</summary>
public abstract class RiskSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Risk, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Risk, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public RiskSpecification And(RiskSpecification other) => new AndRiskSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public RiskSpecification Or(RiskSpecification other) => new OrRiskSpecification(this, other);
}

internal sealed class AndRiskSpecification(RiskSpecification left, RiskSpecification right) : RiskSpecification
{
    public override Expression<Func<Risk, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Risk, bool>>(body, param);
    }
}

internal sealed class OrRiskSpecification(RiskSpecification left, RiskSpecification right) : RiskSpecification
{
    public override Expression<Func<Risk, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Risk, bool>>(body, param);
    }
}