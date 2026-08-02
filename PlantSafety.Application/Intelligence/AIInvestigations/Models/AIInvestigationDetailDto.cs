// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Application
//  File:           AIInvestigationDetailDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single AIInvestigation record, including every
//                  aggregate property plus an ETag. Backs GetAIInvestigationByIdQuery. Distinct
//                  from the pre-existing, simpler AllInvestigationsItemDto used by the list-range
//                  query (GetAllInvestigationsQuery), which is left untouched. Mirrors
//                  PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Models
//                  .RootCauseAnalysisDetailDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Intelligence.AIInvestigations.Models;

/// <summary>A single AIInvestigation read-model record, including the full lifecycle state.</summary>
public sealed record AIInvestigationDetailDto
{
    /// <summary>Record identifier.</summary>
    public required Guid AIInvestigationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Open/InProgress/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Reference identifier of the related safety incident.</summary>
    public required string IncidentReference { get; init; }

    /// <summary>AI model confidence score (0.0-1.0) for the investigation findings.</summary>
    public required double AiConfidenceScore { get; init; }

    /// <summary>AI-generated summary of the investigation.</summary>
    public string? Summary { get; init; }

    /// <summary>Identity of the user or process assigned to the investigation.</summary>
    public required Guid AssignedTo { get; init; }

    /// <summary>UTC timestamp when the investigation was opened.</summary>
    public required DateTime OpenedAtUtc { get; init; }

    /// <summary>Identity of the user who started review, if applicable.</summary>
    public Guid? ReviewStartedBy { get; init; }

    /// <summary>UTC instant review started.</summary>
    public DateTime? ReviewStartedAtUtc { get; init; }

    /// <summary>Identity of the user who closed the investigation, if applicable.</summary>
    public Guid? ClosedBy { get; init; }

    /// <summary>Resolution summary supplied when closing the investigation.</summary>
    public string? ResolutionSummary { get; init; }

    /// <summary>UTC instant the investigation was closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>UTC instant the investigation was archived for compliance retention.</summary>
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
