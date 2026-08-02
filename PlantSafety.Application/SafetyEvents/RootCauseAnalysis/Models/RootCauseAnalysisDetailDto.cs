// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Application
//  File:           RootCauseAnalysisDetailDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single RootCauseAnalysis record, including
//                  every aggregate property plus an ETag. Backs GetRootCauseAnalysisByIdQuery.
//                  Distinct from the pre-existing, simpler RootCauseAnalysisItemDto used by the
//                  list-range query (GetRootCauseAnalysisQuery), which is left untouched. Named
//                  "...DetailDto" rather than the "<Name>ItemDto" convention used elsewhere in this
//                  Phase 4 pass because "RootCauseAnalysisItemDto" was already taken by the
//                  pre-existing list DTO -- a documented, deliberate naming deviation. Mirrors
//                  PlantSafety.Application.Operational.PredictiveMaintenance.Models
//                  .PredictiveMaintenanceAlertItemDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Models;

/// <summary>A single RootCauseAnalysis read-model record, including the full lifecycle state.</summary>
public sealed record RootCauseAnalysisDetailDto
{
    /// <summary>Record identifier.</summary>
    public required Guid RootCauseAnalysisId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (InProgress/PendingReview/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Identifier of the related incident, near-miss, or other safety event, if any.</summary>
    public Guid? RelatedEventId { get; init; }

    /// <summary>Analysis methodology used (e.g., FiveWhys/FishboneDiagram/FaultTree/Bowtie).</summary>
    public required string Methodology { get; init; }

    /// <summary>Identity of the lead analyst.</summary>
    public required Guid LeadAnalyst { get; init; }

    /// <summary>Identity of the user who submitted the analysis for review.</summary>
    public Guid? SubmittedBy { get; init; }

    /// <summary>UTC instant the analysis was submitted for review.</summary>
    public DateTime? SubmittedAtUtc { get; init; }

    /// <summary>Identity of the user who approved the analysis.</summary>
    public Guid? ApprovedBy { get; init; }

    /// <summary>UTC instant the analysis was reviewed and approved (marked completed).</summary>
    public DateTime? CompletedAtUtc { get; init; }

    /// <summary>Identity of the user who formally closed the analysis.</summary>
    public Guid? ClosedBy { get; init; }

    /// <summary>UTC instant the analysis was closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>UTC instant the analysis was archived for compliance retention.</summary>
    public DateTime? ArchivedAtUtc { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
