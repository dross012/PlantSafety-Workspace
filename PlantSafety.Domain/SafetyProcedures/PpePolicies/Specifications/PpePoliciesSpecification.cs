// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Domain
//  File:           PpePoliciesSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for PpePolicies aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.PpePolicies.Specifications;

/// <summary>Abstract base specification for <see cref="PpePolicies"/> aggregate predicate expressions.</summary>
public abstract class PpePoliciesSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates.PpePolicies, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates.PpePolicies, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public PpePoliciesSpecification And(PpePoliciesSpecification other) => new AndPpePoliciesSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public PpePoliciesSpecification Or(PpePoliciesSpecification other) => new OrPpePoliciesSpecification(this, other);
}

internal sealed class AndPpePoliciesSpecification(PpePoliciesSpecification left, PpePoliciesSpecification right) : PpePoliciesSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates.PpePolicies, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates.PpePolicies, bool>>(body, param);
    }
}

internal sealed class OrPpePoliciesSpecification(PpePoliciesSpecification left, PpePoliciesSpecification right) : PpePoliciesSpecification
{
    public override Expression<Func<PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates.PpePolicies, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates.PpePolicies, bool>>(body, param);
    }
}