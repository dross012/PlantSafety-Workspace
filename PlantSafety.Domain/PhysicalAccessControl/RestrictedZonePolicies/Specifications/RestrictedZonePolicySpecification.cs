// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Domain
//  File:           RestrictedZonePolicySpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for RestrictedZonePolicy aggregate predicate
//                  expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Specifications;

/// <summary>Abstract base specification for <see cref="RestrictedZonePolicy"/> aggregate predicate expressions.</summary>
public abstract class RestrictedZonePolicySpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public RestrictedZonePolicySpecification And(RestrictedZonePolicySpecification other) => new AndRestrictedZonePolicySpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public RestrictedZonePolicySpecification Or(RestrictedZonePolicySpecification other) => new OrRestrictedZonePolicySpecification(this, other);
}

internal sealed class AndRestrictedZonePolicySpecification(RestrictedZonePolicySpecification left, RestrictedZonePolicySpecification right) : RestrictedZonePolicySpecification
{
    public override Expression<Func<PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy, bool>>(body, param);
    }
}

internal sealed class OrRestrictedZonePolicySpecification(RestrictedZonePolicySpecification left, RestrictedZonePolicySpecification right) : RestrictedZonePolicySpecification
{
    public override Expression<Func<PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy, bool>>(body, param);
    }
}
