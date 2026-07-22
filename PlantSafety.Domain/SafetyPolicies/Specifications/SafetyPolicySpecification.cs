// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           SafetyPolicySpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for SafetyPolicy aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
using System.Linq.Expressions;
using PlantSafety.Domain.SafetyPolicies.Aggregates;

namespace PlantSafety.Domain.SafetyPolicies.Specifications;

/// <summary>Abstract base specification for SafetyPolicy aggregate predicate expressions.</summary>
public abstract class SafetyPolicySpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<SafetyPolicy, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<SafetyPolicy, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public SafetyPolicySpecification And(SafetyPolicySpecification other) => new AndSafetyPolicySpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public SafetyPolicySpecification Or(SafetyPolicySpecification other) => new OrSafetyPolicySpecification(this, other);
}

internal sealed class AndSafetyPolicySpecification(SafetyPolicySpecification left, SafetyPolicySpecification right) : SafetyPolicySpecification
{
    public override Expression<Func<SafetyPolicy, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<SafetyPolicy, bool>>(body, param);
    }
}

internal sealed class OrSafetyPolicySpecification(SafetyPolicySpecification left, SafetyPolicySpecification right) : SafetyPolicySpecification
{
    public override Expression<Func<SafetyPolicy, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<SafetyPolicy, bool>>(body, param);
    }
}