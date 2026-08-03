// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           IAuditReportFactory.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory contract for drafting AuditReport aggregates. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditReport.Entities;
using PlantSafety.Domain.Auditing.AuditReport.Enums;
using PlantSafety.Domain.Auditing.AuditReport.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditReport.Interfaces;

/// <summary>Factory contract for drafting AuditReport aggregates.</summary>
public interface IAuditReportFactory
{
    /// <summary>Drafts a new AuditReport with the given composed sections.</summary>
    Aggregates.AuditReport Draft(
        Guid tenantId, Guid? siteId, string targetEntityType, Guid targetEntityId,
        AuditReportFormat format, UserId requestedBy, UtcTimestamp requestedAtUtc,
        IEnumerable<AuditReportSection> sections);

    /// <summary>Drafts a new AuditReport, deferring section construction so each section can carry this report's own AuditReportId.</summary>
    Aggregates.AuditReport Draft(
        Guid tenantId, Guid? siteId, string targetEntityType, Guid targetEntityId,
        AuditReportFormat format, UserId requestedBy, UtcTimestamp requestedAtUtc,
        Func<AuditReportId, IEnumerable<AuditReportSection>> sectionsFactory);
}
