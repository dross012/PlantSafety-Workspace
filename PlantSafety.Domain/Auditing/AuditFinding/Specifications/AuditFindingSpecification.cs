// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           AuditFindingSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for AuditFinding aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.Specifications;

/// <summary>Abstract base specification for <see cref="AuditFinding"/> aggregate predicate expressions.</summary>
public abstract class AuditFindingSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Auditing.AuditFinding.Aggregates.AuditFinding, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Auditing.AuditFinding.Aggregates.AuditFinding, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public AuditFindingSpecification And(AuditFindingSpecification other) => new AndAuditFindingSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public AuditFindingSpecification Or(AuditFindingSpecification other) => new OrAuditFindingSpecification(this, other);
}

internal sealed class AndAuditFindingSpecification(AuditFindingSpecification left, AuditFindingSpecification right) : AuditFindingSpecification
{
    public override Expression<Func<PlantSafety.Domain.Auditing.AuditFinding.Aggregates.AuditFinding, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Auditing.AuditFinding.Aggregates.AuditFinding, bool>>(body, param);
    }
}

internal sealed class OrAuditFindingSpecification(AuditFindingSpecification left, AuditFindingSpecification right) : AuditFindingSpecification
{
    public override Expression<Func<PlantSafety.Domain.Auditing.AuditFinding.Aggregates.AuditFinding, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Auditing.AuditFinding.Aggregates.AuditFinding, bool>>(body, param);
    }
}