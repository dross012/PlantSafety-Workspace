// ============================================================================
// File: IncidentsRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.PHMSA.Incidents
// Created: 2026-08-07
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Compliance.PHMSA.Incidents V1 controller. Covers the full DOT Form 5800.1
//              incident-report lifecycle: Create -> Submit -> BeginReview -> Close. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field so clients can supply the
//              optimistic-concurrency token captured from a prior read (e.g.
//              IncidentReportDto.ETag); a stale value results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.Compliance.PHMSA.Incidents.Contracts.Requests;

/// <summary>Request body for filing a new DOT Form 5800.1 incident report.</summary>
public sealed record CreateIncidentReportRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Classification of the incident (Spill, Leak, FireOrExplosion, Injury, PropertyDamage, VehicleAccident, Other).</summary>
    public required string IncidentType { get; init; }

    /// <summary>Optional reference to the classified hazardous material involved.</summary>
    public Guid? MaterialInvolvedId { get; init; }

    /// <summary>Free-text fallback description used when <see cref="MaterialInvolvedId"/> is null.</summary>
    public string? MaterialInvolvedDescription { get; init; }

    /// <summary>Count of injuries associated with the incident.</summary>
    public int Injuries { get; init; }

    /// <summary>Whether the incident involved one or more fatalities.</summary>
    public bool FatalitiesInvolved { get; init; }

    /// <summary>Estimated property damage in USD, if known.</summary>
    public decimal? PropertyDamageEstimateUsd { get; init; }

    /// <summary>Required free-text description of the incident.</summary>
    public required string Narrative { get; init; }

    /// <summary>UTC instant the incident occurred.</summary>
    public required DateTime IncidentDateUtc { get; init; }

    /// <summary>Physical location the incident occurred at.</summary>
    public required string Location { get; init; }
}

/// <summary>Request body for submitting a Draft incident report for review (Draft -&gt; Submitted).</summary>
public sealed record SubmitIncidentReportRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this IncidentReport record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for beginning formal review of a submitted incident report (Submitted -&gt; UnderReview).</summary>
public sealed record BeginIncidentReportReviewRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this IncidentReport record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a reviewed incident report (UnderReview -&gt; Closed).</summary>
public sealed record CloseIncidentReportRequest
{
    /// <summary>Optional resolution notes recorded when the report is closed.</summary>
    public string? ResolutionNotes { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this IncidentReport record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Query-string bound filters for the IncidentReport read-model list endpoint.</summary>
public sealed record IncidentReportsListRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional inclusive start of the incident-date range (UTC).</summary>
    public DateTime? FromUtc { get; init; }

    /// <summary>Optional inclusive end of the incident-date range (UTC).</summary>
    public DateTime? ToUtc { get; init; }

    /// <summary>Optional filter restricting results to (non-)reportable incidents.</summary>
    public bool? IsReportableToPhmsa { get; init; }
}
