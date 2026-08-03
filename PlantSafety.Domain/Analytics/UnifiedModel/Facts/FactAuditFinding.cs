// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.UnifiedModel
//  Layer:          Domain
//  File:           FactAuditFinding.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Fact table for the Audit domain. Grain: one row per audit finding raised
//                  during an audit engagement, sourced from PlantSafety.Domain.Auditing.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.UnifiedModel.Abstractions;
using PlantSafety.Domain.Analytics.UnifiedModel.ValueObjects.Ids;

namespace PlantSafety.Domain.Analytics.UnifiedModel.Facts;

/// <summary>
/// Fact row for a single audit finding raised during an audit engagement. Grain: one row per
/// finding.
/// </summary>
public sealed record FactAuditFinding : IFactRecord
{
    /// <summary>Surrogate key of this fact row.</summary>
    public required UnifiedFactId FactId { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public Guid? SiteKey { get; init; }

    /// <inheritdoc/>
    public required Guid DateKey { get; init; }

    /// <summary>Identifier of the parent audit engagement this finding belongs to.</summary>
    public required Guid AuditEngagementId { get; init; }

    /// <summary>Surrogate key of the associated <c>DimEmployee</c> row for the assigned auditor.</summary>
    public Guid? EmployeeKey { get; init; }

    /// <summary>Finding severity/classification code (e.g. "Minor", "Major", "Critical").</summary>
    public required string SeverityCode { get; init; }

    /// <summary>Finding category (e.g. "Documentation", "ProcessCompliance", "PhysicalHazard").</summary>
    public required string Category { get; init; }

    /// <summary>Finding lifecycle status at capture time.</summary>
    public required string Status { get; init; }

    /// <summary>UTC instant the finding was raised.</summary>
    public required UtcTimestamp RaisedAtUtc { get; init; }

    /// <summary>UTC instant the finding was closed/remediated, if closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; init; }

    /// <summary>Number of days the finding took, or has been open, to remediate.</summary>
    public int? RemediationDays { get; init; }

    /// <summary>True when the finding was remediated before/on its due date.</summary>
    public bool WasRemediatedOnTime { get; init; }

    /// <inheritdoc/>
    public required UtcTimestamp CapturedAtUtc { get; init; }

    /// <inheritdoc/>
    public required Guid SourceAggregateId { get; init; }

    /// <inheritdoc/>
    public string SourceModule => "Auditing";
}
