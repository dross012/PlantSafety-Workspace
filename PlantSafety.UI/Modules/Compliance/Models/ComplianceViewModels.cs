// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Display-ready view models for the Compliance dashboard's
//   three tabs (Regulatory Requirements, Obligations, Tracking). A single
//   unified row shape lets one DataGridComponent/detail-page implementation
//   serve all three heterogeneous source DTOs.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Compliance.Models;

/// <summary>The Compliance dashboard tab/category a row or detail record originated from.</summary>
public enum ComplianceCategory
{
    /// <summary>Regulatory Requirements tab.</summary>
    Requirement,

    /// <summary>Obligations tab.</summary>
    Obligation,

    /// <summary>Tracking tab.</summary>
    Tracking,
}

/// <summary>Unified display row for any of the three Compliance dashboard tabs.</summary>
public sealed class ComplianceRowViewModel
{
    /// <summary>Gets the source category this row was projected from.</summary>
    public required ComplianceCategory Category { get; init; }

    /// <summary>Gets the record identifier (RequirementId/ObligationId/TrackingItemId).</summary>
    public required Guid ItemId { get; init; }

    /// <summary>Gets the primary display title.</summary>
    public required string Title { get; init; }

    /// <summary>Gets the secondary classifier (Jurisdiction/RegulatoryBody/Category).</summary>
    public required string Classifier { get; init; }

    /// <summary>Gets the raw status code.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the StatusIndicatorComponent operational state derived from <see cref="Status"/>.</summary>
    public required string StatusIndicatorState { get; init; }

    /// <summary>Gets the relevant date (EffectiveAtUtc/DueAtUtc/LastUpdatedAtUtc).</summary>
    public required DateTime RelevantDateUtc { get; init; }

    /// <summary>Gets a label describing what <see cref="RelevantDateUtc"/> represents.</summary>
    public required string RelevantDateLabel { get; init; }

    /// <summary>Gets the identifier of the user who owns this record.</summary>
    public required Guid OwnerId { get; init; }
}

/// <summary>Full detail view model for a single Compliance record, regardless of source category.</summary>
public sealed class ComplianceDetailViewModel
{
    /// <summary>Gets the source category this detail was projected from.</summary>
    public required ComplianceCategory Category { get; init; }

    /// <summary>Gets the record identifier.</summary>
    public required Guid ItemId { get; init; }

    /// <summary>Gets the primary display title.</summary>
    public required string Title { get; init; }

    /// <summary>Gets the secondary classifier (Jurisdiction/RegulatoryBody/Category).</summary>
    public required string Classifier { get; init; }

    /// <summary>Gets a label describing what <see cref="Classifier"/> represents (Jurisdiction/Regulatory Body/Category).</summary>
    public required string ClassifierLabel { get; init; }

    /// <summary>Gets the raw status code.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the StatusIndicatorComponent operational state derived from <see cref="Status"/>.</summary>
    public required string StatusIndicatorState { get; init; }

    /// <summary>Gets the relevant date (EffectiveAtUtc/DueAtUtc/LastUpdatedAtUtc).</summary>
    public required DateTime RelevantDateUtc { get; init; }

    /// <summary>Gets a label describing what <see cref="RelevantDateUtc"/> represents.</summary>
    public required string RelevantDateLabel { get; init; }

    /// <summary>Gets the identifier of the user who owns this record.</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }
}
