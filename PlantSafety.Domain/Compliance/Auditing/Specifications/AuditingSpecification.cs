// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Domain
//  File:           AuditingSpecification.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Auditing.Specifications;

/// <summary>Abstract base specification for <see cref="Auditing"/> aggregate predicate expressions.</summary>
public abstract class AuditingSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Auditing, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Auditing, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public AuditingSpecification And(AuditingSpecification other) => new AndAuditingSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public AuditingSpecification Or(AuditingSpecification other) => new OrAuditingSpecification(this, other);
}

internal sealed class AndAuditingSpecification(AuditingSpecification left, AuditingSpecification right) : AuditingSpecification
{
    public override Expression<Func<Auditing, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Auditing, bool>>(body, param);
    }
}

internal sealed class OrAuditingSpecification(AuditingSpecification left, AuditingSpecification right) : AuditingSpecification
{
    public override Expression<Func<Auditing, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<Auditing, bool>>(body, param);
    }
}
