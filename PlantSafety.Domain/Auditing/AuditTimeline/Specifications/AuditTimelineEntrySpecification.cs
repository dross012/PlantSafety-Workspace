// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           AuditTimelineEntrySpecification.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Specifications for filtering composed AuditTimelineEntry sequences (e.g. by
//                  entry type, or by a UTC date range). Additive-only evolution.
// ============================================================================
using System.Linq.Expressions;
using PlantSafety.Domain.Auditing.AuditTimeline.Enums;
using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects;

namespace PlantSafety.Domain.Auditing.AuditTimeline.Specifications;

/// <summary>Abstract base specification for AuditTimelineEntry predicate expressions.</summary>
public abstract class AuditTimelineEntrySpecification
{
    /// <summary>Returns the predicate expression for use in in-memory filtering.</summary>
    public abstract Expression<Func<AuditTimelineEntry, bool>> ToExpression();

    /// <summary>Compiles the expression into a delegate.</summary>
    public Func<AuditTimelineEntry, bool> ToPredicate() => ToExpression().Compile();
}

/// <summary>Matches entries of a specific <see cref="AuditTimelineEntryType"/>.</summary>
public sealed class EntryTypeSpecification(AuditTimelineEntryType entryType) : AuditTimelineEntrySpecification
{
    /// <inheritdoc/>
    public override Expression<Func<AuditTimelineEntry, bool>> ToExpression()
        => e => e.EntryType == entryType;
}

/// <summary>Matches entries whose UtcTimestamp falls within an inclusive UTC date range.</summary>
public sealed class DateRangeSpecification(DateTime fromUtc, DateTime toUtc) : AuditTimelineEntrySpecification
{
    /// <inheritdoc/>
    public override Expression<Func<AuditTimelineEntry, bool>> ToExpression()
        => e => e.UtcTimestamp.Value >= fromUtc && e.UtcTimestamp.Value <= toUtc;
}
