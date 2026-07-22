// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           ActiveMonitoringAuditingSpecification.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for ActiveMonitoringAuditing aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Specifications;

/// <summary>Abstract base specification for <see cref="ActiveMonitoringAuditing"/> aggregate predicate expressions.</summary>
public abstract class ActiveMonitoringAuditingSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Aggregates.ActiveMonitoringAuditing, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Aggregates.ActiveMonitoringAuditing, bool> ToPredicate() => ToExpression().Compile();

    /// <summary>Combines this specification with <paramref name="other"/> using logical AND.</summary>
    public ActiveMonitoringAuditingSpecification And(ActiveMonitoringAuditingSpecification other) => new AndActiveMonitoringAuditingSpecification(this, other);

    /// <summary>Combines this specification with <paramref name="other"/> using logical OR.</summary>
    public ActiveMonitoringAuditingSpecification Or(ActiveMonitoringAuditingSpecification other) => new OrActiveMonitoringAuditingSpecification(this, other);
}

internal sealed class AndActiveMonitoringAuditingSpecification(ActiveMonitoringAuditingSpecification left, ActiveMonitoringAuditingSpecification right) : ActiveMonitoringAuditingSpecification
{
    public override Expression<Func<PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Aggregates.ActiveMonitoringAuditing, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.AndAlso(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Aggregates.ActiveMonitoringAuditing, bool>>(body, param);
    }
}

internal sealed class OrActiveMonitoringAuditingSpecification(ActiveMonitoringAuditingSpecification left, ActiveMonitoringAuditingSpecification right) : ActiveMonitoringAuditingSpecification
{
    public override Expression<Func<PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Aggregates.ActiveMonitoringAuditing, bool>> ToExpression()
    {
        var l = left.ToExpression();
        var r = right.ToExpression();
        var param = l.Parameters[0];
        var body = Expression.OrElse(l.Body, Expression.Invoke(r, param));
        return Expression.Lambda<Func<PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Aggregates.ActiveMonitoringAuditing, bool>>(body, param);
    }
}