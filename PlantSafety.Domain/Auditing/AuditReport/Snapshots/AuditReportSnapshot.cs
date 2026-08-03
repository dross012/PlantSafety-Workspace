// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportSnapshot.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Immutable snapshot of an AuditReport aggregate for fast rehydration and for
//                  use as the "snapshot baseline" export artifact described in Handbook Phase 5
//                  §6.2 (a point-in-time export of an aggregate's audit state). Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditReport.Enums;
using PlantSafety.Domain.Auditing.AuditReport.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditReport.Snapshots;

/// <summary>Immutable snapshot of an AuditReport aggregate.</summary>
public sealed record AuditReportSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public AuditReportId AuditReportId { get; init; }

    /// <summary>Type discriminator of the documented entity.</summary>
    public required string TargetEntityType { get; init; }

    /// <summary>Identity of the documented entity.</summary>
    public Guid TargetEntityId { get; init; }

    /// <summary>Output format at snapshot time.</summary>
    public AuditReportFormat Format { get; init; } = AuditReportFormat.Default;

    /// <summary>Lifecycle status at snapshot time.</summary>
    public AuditReportStatus Status { get; init; } = AuditReportStatus.Default;

    /// <summary>Number of composed sections at snapshot time.</summary>
    public int SectionCount { get; init; }

    /// <summary>Identity of the user or system that requested the report.</summary>
    public UserId RequestedBy { get; init; }

    /// <summary>UTC instant the snapshot was captured.</summary>
    public UtcTimestamp SnapshotAtUtc { get; init; }
}
