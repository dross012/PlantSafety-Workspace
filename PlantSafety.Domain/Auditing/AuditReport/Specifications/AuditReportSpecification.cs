// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportSpecification.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Abstract base specification for AuditReport aggregate predicate expressions.
//                  Additive-only evolution.
// ============================================================================
using System.Linq.Expressions;

namespace PlantSafety.Domain.Auditing.AuditReport.Specifications;

/// <summary>Abstract base specification for AuditReport aggregate predicate expressions.</summary>
public abstract class AuditReportSpecification
{
    /// <summary>Returns the predicate expression for use in repository queries.</summary>
    public abstract Expression<Func<Aggregates.AuditReport, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate for in-memory filtering.</summary>
    public Func<Aggregates.AuditReport, bool> ToPredicate() => ToExpression().Compile();
}

/// <summary>Matches AuditReport aggregates for a specific target entity.</summary>
public sealed class AuditReportForTargetSpecification(string targetEntityType, Guid targetEntityId) : AuditReportSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<Aggregates.AuditReport, bool>> ToExpression()
        => r => r.TargetEntityType == targetEntityType && r.TargetEntityId == targetEntityId;
}

/// <summary>Matches successfully generated AuditReport aggregates.</summary>
public sealed class GeneratedAuditReportSpecification : AuditReportSpecification
{
    /// <inheritdoc/>
    public override Expression<Func<Aggregates.AuditReport, bool>> ToExpression()
        => r => r.Status == Enums.AuditReportStatus.Generated;
}
