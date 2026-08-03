// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportGeneratedEvent.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when an AuditReport is successfully rendered to its target format
//                  (PDF/CSV/JSON). Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditReport.Enums;
using PlantSafety.Domain.Auditing.AuditReport.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditReport.Events;

/// <summary>Raised when an AuditReport is successfully generated.</summary>
public sealed record AuditReportGeneratedEvent(
    Guid EventId,
    AuditReportId AuditReportId,
    string TargetEntityType,
    Guid TargetEntityId,
    AuditReportFormat Format,
    long ContentSizeBytes,
    UtcTimestamp GeneratedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAuditReportDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AuditReportGenerated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => GeneratedAtUtc;
}
