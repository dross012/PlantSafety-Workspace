// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.DisposalCertificates
//  Layer:          Domain
//  File:           DisposalCertificateSpecification.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Abstract base specification for DisposalCertificate aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Aggregates;

namespace PlantSafety.Domain.HazardousWasteManagement.DisposalCertificates.Specifications;

/// <summary>Abstract base specification for DisposalCertificate aggregate predicate expressions.</summary>
public abstract class DisposalCertificateSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<DisposalCertificate, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<DisposalCertificate, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public DisposalCertificateSpecification And(DisposalCertificateSpecification other) => new AndDisposalCertificateSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public DisposalCertificateSpecification Or(DisposalCertificateSpecification other) => new OrDisposalCertificateSpecification(this, other);
}

internal sealed class AndDisposalCertificateSpecification(DisposalCertificateSpecification left, DisposalCertificateSpecification right) : DisposalCertificateSpecification
{
    public override Expression<Func<DisposalCertificate, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<DisposalCertificate, bool>>(body, param);
    }
}

internal sealed class OrDisposalCertificateSpecification(DisposalCertificateSpecification left, DisposalCertificateSpecification right) : DisposalCertificateSpecification
{
    public override Expression<Func<DisposalCertificate, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<DisposalCertificate, bool>>(body, param);
    }
}
