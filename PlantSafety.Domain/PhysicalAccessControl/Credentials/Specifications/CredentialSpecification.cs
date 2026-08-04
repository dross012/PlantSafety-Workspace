// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.Credentials
//  Layer:          Domain
//  File:           CredentialSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for Credential aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.Credentials.Specifications;

/// <summary>Abstract base specification for <see cref="Credential"/> aggregate predicate expressions.</summary>
public abstract class CredentialSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public CredentialSpecification And(CredentialSpecification other) => new AndCredentialSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public CredentialSpecification Or(CredentialSpecification other) => new OrCredentialSpecification(this, other);
}

internal sealed class AndCredentialSpecification(CredentialSpecification left, CredentialSpecification right) : CredentialSpecification
{
    public override Expression<Func<PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential, bool>>(body, param);
    }
}

internal sealed class OrCredentialSpecification(CredentialSpecification left, CredentialSpecification right) : CredentialSpecification
{
    public override Expression<Func<PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.PhysicalAccessControl.Credentials.Aggregates.Credential, bool>>(body, param);
    }
}
