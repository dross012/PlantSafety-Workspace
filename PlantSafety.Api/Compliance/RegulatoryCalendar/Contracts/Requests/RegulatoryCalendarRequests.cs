// ============================================================================
// File: RegulatoryCalendarRequests.cs
// Project: PlantSafety.Api
// Module: Compliance.RegulatoryCalendar
// Created: 2026-08-02
// Description: Request DTOs bound from the HTTP request body/query string for the
//              Compliance.RegulatoryCalendar V1 controller. Covers the full lifecycle:
//              Create -> Activate -> RecordCompliance -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests.cs -- every
//              mutating lifecycle request carries an optional ETag field so clients can supply the
//              optimistic-concurrency token captured from a prior read (e.g.
//              RegulatoryCalendarItemDto.ETag); a stale value results in a concurrency-conflict error.
// ============================================================================

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Contracts.Requests;

/// <summary>Request body for submitting a new RegulatoryCalendar obligation record.</summary>
public sealed record CreateRegulatoryCalendarRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable title of the regulatory event/obligation.</summary>
    public required string EventTitle { get; init; }

    /// <summary>Governing regulatory body (e.g., "OSHA", "EPA", "State DOL").</summary>
    public required string RegulatoryBody { get; init; }

    /// <summary>Jurisdiction code the obligation applies to (e.g., "US-CA", "US-FED").</summary>
    public required string JurisdictionCode { get; init; }

    /// <summary>Compliance category (e.g., Reporting, Inspection, Renewal, Filing).</summary>
    public required string ComplianceCategory { get; init; }

    /// <summary>Identity of the party accountable for meeting this obligation.</summary>
    public required Guid ResponsiblePartyId { get; init; }

    /// <summary>UTC timestamp the obligation is due, if known.</summary>
    public DateTime? DueAtUtc { get; init; }

    /// <summary>Number of days before DueAtUtc that reminders should fire. Defaults to 30.</summary>
    public int ReminderLeadDays { get; init; } = 30;
}

/// <summary>Request body for activating a RegulatoryCalendar obligation (Created -&gt; Active).</summary>
public sealed record ActivateRegulatoryCalendarRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for recording compliance with an obligation (Active -&gt; Completed).</summary>
public sealed record RecordRegulatoryCalendarComplianceRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for formally closing a completed obligation (Completed -&gt; Closed).</summary>
public sealed record CloseRegulatoryCalendarRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for archiving a closed obligation for compliance retention (Closed -&gt; Archived).</summary>
public sealed record ArchiveRegulatoryCalendarRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Query-string bound filters for the RegulatoryCalendar read-model list endpoint.</summary>
public sealed record RegulatoryCalendarListRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Inclusive start of the date range (UTC). Defaults to 90 days before now.</summary>
    public DateTime? From { get; init; }

    /// <summary>Inclusive end of the date range (UTC). Defaults to 90 days after now.</summary>
    public DateTime? To { get; init; }
}
