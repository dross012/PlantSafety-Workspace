// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-29
// FILE DESCRIPTION: Read-model DTO for a single Accident record. Closes the Phase 4
//   AccidentManagement gap: there was previously no way for an API client to read an Accident's
//   current state (and thus obtain its ETag before writing). Property names match
//   usp_Accident_GetById's projected column aliases exactly for direct Dapper mapping. Mirrors
//   PlantSafety.Application.RiskManagement.Models.RiskItemDto.
// SchemaVersion: 1

namespace PlantSafety.Application.AccidentManagement.Models;

/// <summary>A single Accident read-model record.</summary>
public sealed record AccidentItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid AccidentId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>Human-readable accident title/summary.</summary>
    public required string AccidentTitle { get; init; }

    /// <summary>Free-form location description.</summary>
    public required string Location { get; init; }

    /// <summary>Severity classification code.</summary>
    public required string Severity { get; init; }

    /// <summary>Number of persons injured.</summary>
    public required int InjuredPersonCount { get; init; }

    /// <summary>UTC instant the accident occurred.</summary>
    public required DateTime OccurredAtUtc { get; init; }

    /// <summary>Investigation phase category code.</summary>
    public required string Category { get; init; }

    /// <summary>Whether the site has been secured and triaged.</summary>
    public required bool SiteSecuredAndTriaged { get; init; }

    /// <summary>Identity of the assigned investigator, if any.</summary>
    public Guid? InvestigatorId { get; init; }

    /// <summary>UTC instant an investigator was assigned.</summary>
    public DateTime? InvestigatorAssignedAtUtc { get; init; }

    /// <summary>Root cause category code, once recorded.</summary>
    public string? RootCause { get; init; }

    /// <summary>Free-text root cause notes.</summary>
    public string? RootCauseNotes { get; init; }

    /// <summary>UTC instant the root cause was recorded.</summary>
    public DateTime? RootCauseRecordedAtUtc { get; init; }

    /// <summary>Whether regulatory filing is required for this accident.</summary>
    public required bool RegulatoryFilingRequired { get; init; }

    /// <summary>Regulatory agency the report was filed with, if any.</summary>
    public string? RegulatoryAgency { get; init; }

    /// <summary>Regulatory filing reference number.</summary>
    public string? RegulatoryReferenceNumber { get; init; }

    /// <summary>UTC instant the regulatory report was filed.</summary>
    public DateTime? RegulatoryFiledAtUtc { get; init; }

    /// <summary>UTC instant the investigation was closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>Identity of the accountable owner.</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>Whether formal review/acknowledgment is required.</summary>
    public required bool ReviewRequired { get; init; }

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
