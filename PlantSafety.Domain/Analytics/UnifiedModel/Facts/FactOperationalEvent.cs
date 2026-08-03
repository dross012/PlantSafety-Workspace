// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Domain
//  File:           FactOperationalEvent.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Fact table for the Production/Operational domain. Grain: one row per
//                  operational safety event (inspection, checklist completion, JSA, PPE
//                  compliance check, environmental reading) across PlantSafety.Domain.Operational.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.UnifiedModel.Abstractions;
using PlantSafety.Domain.Analytics.UnifiedModel.ValueObjects.Ids;

namespace PlantSafety.Domain.Analytics.UnifiedModel.Facts;

/// <summary>
/// Fact row for a single Production/Operational-domain event (inspection completed, checklist
/// submitted, job safety analysis performed, PPE compliance check, environmental reading).
/// Grain: one row per operational event occurrence.
/// </summary>
public sealed record FactOperationalEvent : IFactRecord
{
    /// <summary>Surrogate key of this fact row.</summary>
    public required UnifiedFactId FactId { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public Guid? SiteKey { get; init; }

    /// <inheritdoc/>
    public required Guid DateKey { get; init; }

    /// <summary>Surrogate key of the associated <c>DimAsset</c> row, if equipment/line-scoped.</summary>
    public Guid? AssetKey { get; init; }

    /// <summary>Surrogate key of the associated <c>DimEmployee</c> row for the performing employee.</summary>
    public Guid? EmployeeKey { get; init; }

    /// <summary>Sub-context this event originated from (Inspections, Checklists, JobSafetyAnalysis, PPEManagement, EnvironmentalMonitoring, IndustrialHygiene, etc.).</summary>
    public required string OperationalSubContext { get; init; }

    /// <summary>Event type code (e.g. "InspectionCompleted", "ChecklistSubmitted", "JsaPerformed").</summary>
    public required string EventType { get; init; }

    /// <summary>UTC instant the operational event occurred.</summary>
    public required UtcTimestamp OccurredAtUtc { get; init; }

    /// <summary>Outcome/result code of the event (e.g. "Pass", "Fail", "Conditional").</summary>
    public string? OutcomeCode { get; init; }

    /// <summary>Number of discrepancies/non-conformances recorded, if applicable.</summary>
    public int DiscrepancyCount { get; init; }

    /// <summary>True when the event outcome was fully compliant.</summary>
    public bool IsCompliant { get; init; }

    /// <inheritdoc/>
    public required UtcTimestamp CapturedAtUtc { get; init; }

    /// <inheritdoc/>
    public required Guid SourceAggregateId { get; init; }

    /// <inheritdoc/>
    public string SourceModule => "Operational";
}
