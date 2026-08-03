// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportGenerationFailedEvent.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when AuditReport rendering fails so the failure is durably tracked
//                  rather than silently dropped. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditReport.Enums;
using PlantSafety.Domain.Auditing.AuditReport.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditReport.Events;

/// <summary>Raised when AuditReport rendering fails.</summary>
public sealed record AuditReportGenerationFailedEvent(
    Guid EventId,
    AuditReportId AuditReportId,
    AuditReportFormat Format,
    string Reason,
    UtcTimestamp FailedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAuditReportDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AuditReportGenerationFailed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => FailedAtUtc;
}
