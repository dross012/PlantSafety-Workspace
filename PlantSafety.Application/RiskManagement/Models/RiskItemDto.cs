// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-29
// FILE DESCRIPTION: Read-model DTO for a single Risk record. Closes the Phase 4 RiskManagement
//   gap: there was previously no way for an API client to read a Risk's current state (and thus
//   obtain its ETag before writing). Property names match usp_Risk_GetById's projected column
//   aliases exactly for direct Dapper mapping.
// SchemaVersion: 1

namespace PlantSafety.Application.RiskManagement.Models;

/// <summary>A single Risk read-model record.</summary>
public sealed record RiskItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid RiskId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Identified/Scored/Prioritized/ControlsInProgress/Mitigated/UnderReview/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>Human-readable hazard title/summary.</summary>
    public required string RiskTitle { get; init; }

    /// <summary>Risk level category (Low/Medium/High/Critical), derived from scoring.</summary>
    public required string Category { get; init; }

    /// <summary>Free-form location description (building/zone/equipment).</summary>
    public required string Location { get; init; }

    /// <summary>Hazard source classification.</summary>
    public required string HazardSource { get; init; }

    /// <summary>Identity of the accountable owner.</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>Current severity component of the Severity x Likelihood matrix.</summary>
    public int? CurrentSeverity { get; init; }

    /// <summary>Current likelihood component of the Severity x Likelihood matrix.</summary>
    public int? CurrentLikelihood { get; init; }

    /// <summary>P1-P4 remediation priority.</summary>
    public string? Priority { get; init; }

    /// <summary>UTC instant the finding was documented.</summary>
    public DateTime? DocumentedAtUtc { get; init; }

    /// <summary>Residual severity after controls are applied.</summary>
    public int? ResidualSeverity { get; init; }

    /// <summary>Residual likelihood after controls are applied.</summary>
    public int? ResidualLikelihood { get; init; }

    /// <summary>UTC instant residual-risk effectiveness was last validated.</summary>
    public DateTime? LastValidatedAtUtc { get; init; }

    /// <summary>Periodic review cadence.</summary>
    public required string ReviewCadence { get; init; }

    /// <summary>UTC instant of the last completed review.</summary>
    public DateTime? LastReviewedAtUtc { get; init; }

    /// <summary>UTC instant the next review is due.</summary>
    public DateTime? NextReviewDueUtc { get; init; }

    /// <summary>Whether formal review/acknowledgment is required.</summary>
    public bool ReviewRequired { get; init; }

    /// <summary>Number of completed review cycles.</summary>
    public int ReviewCount { get; init; }

    /// <summary>Revision counter, bumped on each completed review.</summary>
    public int RiskVersion { get; init; }

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
