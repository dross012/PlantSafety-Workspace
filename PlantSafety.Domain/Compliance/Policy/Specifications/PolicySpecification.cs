// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Domain
//  File:           PolicySpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Policy.Specifications;

/// <summary>Abstract base specification for <see cref="Policy"/> aggregate predicate expressions.</summary>
public abstract class PolicySpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Compliance.Policy.Aggregates.Policy, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Compliance.Policy.Aggregates.Policy, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PolicySpecification And(PolicySpecification other) => new AndPolicySpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PolicySpecification Or(PolicySpecification other) => new OrPolicySpecification(this, other);
}

internal sealed class AndPolicySpecification(PolicySpecification left, PolicySpecification right) : PolicySpecification
{
    public override Expression<Func<PlantSafety.Domain.Compliance.Policy.Aggregates.Policy, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Compliance.Policy.Aggregates.Policy, bool>>(body, param);
    }
}

internal sealed class OrPolicySpecification(PolicySpecification left, PolicySpecification right) : PolicySpecification
{
    public override Expression<Func<PlantSafety.Domain.Compliance.Policy.Aggregates.Policy, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Compliance.Policy.Aggregates.Policy, bool>>(body, param);
    }
}
