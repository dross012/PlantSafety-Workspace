// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Application
//  File:           RCADetailDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single RCA record, including every aggregate
//                  property plus an ETag. Backs GetRCAByIdQuery. Distinct from the pre-existing,
//                  simpler RCAItemDto used by the date-range list query (GetRCAQuery), which is
//                  left entirely untouched by this pass. Named "...DetailDto" rather than the
//                  "<Name>ItemDto" convention used elsewhere in this Phase 4 pass because
//                  "RCAItemDto" was already taken by the pre-existing list DTO -- a documented,
//                  deliberate naming deviation, mirroring the identical precedent already
//                  established by
//                  PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Models
//                  .RootCauseAnalysisDetailDto for the sibling SafetyEvents.RootCauseAnalysis
//                  module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ContinuousImprovement.RCA.Models;

/// <summary>A single RCA read-model record, including the full lifecycle state.</summary>
public sealed record RCADetailDto
{
    /// <summary>RCA record identifier.</summary>
    public required Guid RcaId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Problem statement being analyzed.</summary>
    public required string ProblemStatement { get; init; }

    /// <summary>Analysis methodology used (e.g., FiveWhys/FishboneDiagram/FaultTree/Bowtie).</summary>
    public required string Methodology { get; init; }

    /// <summary>Current lifecycle status (Initiated/InProgress/PendingApproval/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the RCA was initiated.</summary>
    public required DateTime InitiatedAtUtc { get; init; }

    /// <summary>Identity of the user who owns the RCA.</summary>
    public required Guid OwnerUserId { get; init; }

    /// <summary>Identity of the user who started the analysis.</summary>
    public Guid? StartedBy { get; init; }

    /// <summary>UTC instant analysis work began.</summary>
    public DateTime? StartedAtUtc { get; init; }

    /// <summary>Identity of the user who submitted the RCA for approval.</summary>
    public Guid? SubmittedBy { get; init; }

    /// <summary>Findings recorded when the RCA was submitted for approval, if any.</summary>
    public string? Findings { get; init; }

    /// <summary>UTC instant the RCA was submitted for approval.</summary>
    public DateTime? SubmittedAtUtc { get; init; }

    /// <summary>Identity of the user who formally closed the RCA.</summary>
    public Guid? ClosedBy { get; init; }

    /// <summary>Root cause summary recorded when the RCA was closed, if any.</summary>
    public string? RootCauseSummary { get; init; }

    /// <summary>UTC instant the RCA was closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>UTC instant the RCA was archived for compliance retention.</summary>
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
