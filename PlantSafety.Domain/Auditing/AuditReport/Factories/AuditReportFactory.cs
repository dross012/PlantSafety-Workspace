// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportFactory.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for drafting AuditReport aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditReport.Entities;
using PlantSafety.Domain.Auditing.AuditReport.Enums;
using PlantSafety.Domain.Auditing.AuditReport.Interfaces;
using PlantSafety.Domain.Auditing.AuditReport.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditReport.Factories;

/// <summary>Default factory implementation for drafting AuditReport aggregates.</summary>
public sealed class AuditReportFactory : IAuditReportFactory
{
    /// <inheritdoc/>
    public Aggregates.AuditReport Draft(
        Guid tenantId, Guid? siteId, string targetEntityType, Guid targetEntityId,
        AuditReportFormat format, UserId requestedBy, UtcTimestamp requestedAtUtc,
        IEnumerable<AuditReportSection> sections)
        => Aggregates.AuditReport.Draft(tenantId, siteId, targetEntityType, targetEntityId, format, requestedBy, requestedAtUtc, sections);

    /// <inheritdoc/>
    public Aggregates.AuditReport Draft(
        Guid tenantId, Guid? siteId, string targetEntityType, Guid targetEntityId,
        AuditReportFormat format, UserId requestedBy, UtcTimestamp requestedAtUtc,
        Func<AuditReportId, IEnumerable<AuditReportSection>> sectionsFactory)
        => Aggregates.AuditReport.Draft(tenantId, siteId, targetEntityType, targetEntityId, format, requestedBy, requestedAtUtc, sectionsFactory);
}
