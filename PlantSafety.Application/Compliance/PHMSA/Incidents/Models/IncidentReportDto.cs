// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Application
//  File:           IncidentReportDto.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single IncidentReport record. Backs both
//                  GetIncidentReportQuery and ListIncidentReportsQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.PHMSA.Incidents.Models;

/// <summary>A single IncidentReport read-model record.</summary>
public sealed record IncidentReportDto
{
    /// <summary>Record identifier.</summary>
    public required Guid IncidentReportId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Draft/Submitted/UnderReview/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>Classification of the incident (Spill, Leak, FireOrExplosion, etc.).</summary>
    public required string IncidentType { get; init; }

    /// <summary>Optional reference to the classified hazardous material involved.</summary>
    public Guid? MaterialInvolvedId { get; init; }

    /// <summary>Free-text fallback description used when <see cref="MaterialInvolvedId"/> is null.</summary>
    public string? MaterialInvolvedDescription { get; init; }

    /// <summary>Count of injuries associated with the incident.</summary>
    public required int Injuries { get; init; }

    /// <summary>Whether the incident involved one or more fatalities.</summary>
    public required bool FatalitiesInvolved { get; init; }

    /// <summary>Estimated property damage in USD, if known.</summary>
    public decimal? PropertyDamageEstimateUsd { get; init; }

    /// <summary>Free-text description of the incident.</summary>
    public required string Narrative { get; init; }

    /// <summary>UTC instant the incident occurred.</summary>
    public required DateTime IncidentDateUtc { get; init; }

    /// <summary>Physical location the incident occurred at.</summary>
    public required string Location { get; init; }

    /// <summary>Whether this incident meets the PHMSA reporting threshold.</summary>
    public required bool IsReportableToPhmsa { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>UTC timestamp when the record was last updated, if ever.</summary>
    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>Identity of the user who last updated this record, if ever.</summary>
    public Guid? UpdatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
