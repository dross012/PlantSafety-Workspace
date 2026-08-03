// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Domain
//  File:           FactRiskAssessment.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Fact table for the Risk domain. Grain: one row per Risk aggregate
//                  scoring/prioritization/review event (see PlantSafety.Domain.RiskManagement.
//                  Aggregates.Risk). Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.UnifiedModel.Abstractions;
using PlantSafety.Domain.Analytics.UnifiedModel.ValueObjects.Ids;

namespace PlantSafety.Domain.Analytics.UnifiedModel.Facts;

/// <summary>
/// Fact row for a single Risk-domain scoring/review event. Grain: one row per
/// scored/prioritized/reviewed instant of a <c>Risk</c> aggregate.
/// </summary>
public sealed record FactRiskAssessment : IFactRecord
{
    /// <summary>Surrogate key of this fact row.</summary>
    public required UnifiedFactId FactId { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public Guid? SiteKey { get; init; }

    /// <inheritdoc/>
    public required Guid DateKey { get; init; }

    /// <summary>Surrogate key of the associated <c>DimEmployee</c> row for the risk owner.</summary>
    public Guid? EmployeeKey { get; init; }

    /// <summary>Severity component of the Severity x Likelihood score at capture time.</summary>
    public required int Severity { get; init; }

    /// <summary>Likelihood component of the Severity x Likelihood score at capture time.</summary>
    public required int Likelihood { get; init; }

    /// <summary>Composite Severity x Likelihood numeric score.</summary>
    public required int CompositeScore { get; init; }

    /// <summary>RiskLevel code at capture time (Low/Medium/High/Critical).</summary>
    public required string RiskLevelCode { get; init; }

    /// <summary>RiskPriority code (P1-P4), if assigned.</summary>
    public string? PriorityCode { get; init; }

    /// <summary>Number of control measures applied at capture time.</summary>
    public int ControlMeasureCount { get; init; }

    /// <summary>True when this row represents a residual (post-control) score rather than the initial score.</summary>
    public bool IsResidualScore { get; init; }

    /// <summary>Risk lifecycle status at capture time.</summary>
    public required string Status { get; init; }

    /// <inheritdoc/>
    public required UtcTimestamp CapturedAtUtc { get; init; }

    /// <inheritdoc/>
    public required Guid SourceAggregateId { get; init; }

    /// <inheritdoc/>
    public string SourceModule => "RiskManagement";
}
