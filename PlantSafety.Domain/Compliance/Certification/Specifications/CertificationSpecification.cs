// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Domain
//  File:           CertificationSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Certification.Specifications;

/// <summary>Abstract base specification for <see cref="Certification"/> aggregate predicate expressions.</summary>
public abstract class CertificationSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Certification, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Certification, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public CertificationSpecification And(CertificationSpecification other) => new AndCertificationSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public CertificationSpecification Or(CertificationSpecification other) => new OrCertificationSpecification(this, other);
}

internal sealed class AndCertificationSpecification(CertificationSpecification left, CertificationSpecification right) : CertificationSpecification
{
    public override Expression<Func<Certification, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Certification, bool>>(body, param);
    }
}

internal sealed class OrCertificationSpecification(CertificationSpecification left, CertificationSpecification right) : CertificationSpecification
{
    public override Expression<Func<Certification, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Certification, bool>>(body, param);
    }
}
