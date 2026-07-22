// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           AuditEngagementSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for AuditEngagement aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Specifications;

/// <summary>Abstract base specification for <see cref="AuditEngagement"/> aggregate predicate expressions.</summary>
public abstract class AuditEngagementSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public AuditEngagementSpecification And(AuditEngagementSpecification other) => new AndAuditEngagementSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public AuditEngagementSpecification Or(AuditEngagementSpecification other) => new OrAuditEngagementSpecification(this, other);
}

internal sealed class AndAuditEngagementSpecification(AuditEngagementSpecification left, AuditEngagementSpecification right) : AuditEngagementSpecification
{
    public override Expression<Func<PlantSafety.Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement, bool>>(body, param);
    }
}

internal sealed class OrAuditEngagementSpecification(AuditEngagementSpecification left, AuditEngagementSpecification right) : AuditEngagementSpecification
{
    public override Expression<Func<PlantSafety.Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement, bool>>(body, param);
    }
}