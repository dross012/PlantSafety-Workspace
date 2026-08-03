// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Domain
//  File:           FactContinuousImprovementAction.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Fact table for the ContinuousImprovement domain. Grain: one row per
//                  corrective/preventive action (CAPA) or root-cause-analysis (RCA) finding.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.UnifiedModel.Abstractions;
using PlantSafety.Domain.Analytics.UnifiedModel.ValueObjects.Ids;

namespace PlantSafety.Domain.Analytics.UnifiedModel.Facts;

/// <summary>
/// Fact row for a single ContinuousImprovement-domain action (CAPA action item or RCA finding).
/// Grain: one row per action/finding lifecycle terminal state.
/// </summary>
public sealed record FactContinuousImprovementAction : IFactRecord
{
    /// <summary>Surrogate key of this fact row.</summary>
    public required UnifiedFactId FactId { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public Guid? SiteKey { get; init; }

    /// <inheritdoc/>
    public required Guid DateKey { get; init; }

    /// <summary>Surrogate key of the associated <c>DimEmployee</c> row for the assigned owner.</summary>
    public Guid? EmployeeKey { get; init; }

    /// <summary>Sub-context this row originated from: "CAPA" or "RCA".</summary>
    public required string ImprovementSubContext { get; init; }

    /// <summary>Action/finding type code (e.g. "CorrectiveAction", "PreventiveAction", "RootCauseFinding").</summary>
    public required string ActionType { get; init; }

    /// <summary>Lifecycle status at capture time.</summary>
    public required string Status { get; init; }

    /// <summary>UTC instant the action/finding was opened.</summary>
    public required UtcTimestamp OpenedAtUtc { get; init; }

    /// <summary>UTC instant the action/finding was closed, if closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; init; }

    /// <summary>Number of days the action/finding took, or has been open, to resolve.</summary>
    public int? CycleTimeDays { get; init; }

    /// <summary>True when the action/finding was closed before/on its due date.</summary>
    public bool WasClosedOnTime { get; init; }

    /// <summary>True when effectiveness of the corrective action was verified.</summary>
    public bool EffectivenessVerified { get; init; }

    /// <inheritdoc/>
    public required UtcTimestamp CapturedAtUtc { get; init; }

    /// <inheritdoc/>
    public required Guid SourceAggregateId { get; init; }

    /// <inheritdoc/>
    public string SourceModule => "ContinuousImprovement";
}
