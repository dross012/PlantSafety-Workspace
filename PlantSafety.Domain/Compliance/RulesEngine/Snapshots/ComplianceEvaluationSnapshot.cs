// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceEvaluationSnapshot.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Immutable, point-in-time snapshot of a ComplianceEvaluation aggregate for
//                  fast rehydration and for use as the "state change history" and "snapshot
//                  baseline" building block consumed by AuditTimeline (§6.1) and AuditReport
//                  (§6.2). Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.RulesEngine.Enums;
using PlantSafety.Domain.Compliance.RulesEngine.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.RulesEngine.Snapshots;

/// <summary>Immutable snapshot of a ComplianceEvaluation aggregate.</summary>
public sealed record ComplianceEvaluationSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public ComplianceEvaluationId ComplianceEvaluationId { get; init; }

    /// <summary>Type discriminator of the evaluated entity.</summary>
    public required string TargetEntityType { get; init; }

    /// <summary>Identity of the evaluated entity.</summary>
    public Guid TargetEntityId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public ComplianceEvaluationStatus Status { get; init; } = ComplianceEvaluationStatus.Default;

    /// <summary>Number of rules evaluated at snapshot time.</summary>
    public int RulesEvaluated { get; init; }

    /// <summary>Number of rules failed at snapshot time.</summary>
    public int RulesFailed { get; init; }

    /// <summary>UTC instant the snapshot was captured.</summary>
    public UtcTimestamp SnapshotAtUtc { get; init; }

    /// <summary>Identity of the user or system that triggered the evaluation.</summary>
    public UserId TriggeredBy { get; init; }
}
