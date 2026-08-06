// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-05
// FILE DESCRIPTION: Client-side DTOs for the Compliance.RegulatoryCalendar
//   write/lifecycle surface (api/v1/compliance/regulatorycalendar) —
//   Create -> Activate -> RecordCompliance -> Close -> Archive, plus a real
//   by-id GET. This is a distinct, generic RegulatoryCalendar aggregate from
//   the 9 read-only Deadlines/Inspections/Renewals source verticals backing
//   RegulatoryCalendarListPage/RegulatoryCalendarDetailPage — those have no
//   write commands; this is where CreateRegulatoryCalendarCommand and its
//   lifecycle actions actually live. Mirrors the shapes in
//   PlantSafety.Api.Compliance.RegulatoryCalendar.Contracts.Requests.
//   RegulatoryCalendarRequests and
//   PlantSafety.Application.Compliance.RegulatoryCalendar.Models/Commands.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace PlantSafety.UI.Modules.RegulatoryCalendar.Models;

/// <summary>Full read-model record for a single generic RegulatoryCalendar obligation (mirrors <c>RegulatoryCalendarItemDto</c>), backing GET v1/compliance/regulatorycalendar/{id}.</summary>
public sealed record RegulatoryCalendarItemDto
{
    /// <summary>Record identifier.</summary>
    public Guid RegulatoryCalendarId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Human-readable title of the regulatory event/obligation.</summary>
    public string EventTitle { get; init; } = string.Empty;

    /// <summary>Governing regulatory body (e.g., "OSHA", "EPA", "State DOL").</summary>
    public string RegulatoryBody { get; init; } = string.Empty;

    /// <summary>Jurisdiction code the obligation applies to (e.g., "US-CA", "US-FED").</summary>
    public string JurisdictionCode { get; init; } = string.Empty;

    /// <summary>Compliance category (e.g., Reporting, Inspection, Renewal, Filing).</summary>
    public string ComplianceCategory { get; init; } = string.Empty;

    /// <summary>Identity of the party accountable for meeting this obligation.</summary>
    public Guid? ResponsiblePartyId { get; init; }

    /// <summary>UTC timestamp the obligation is due.</summary>
    public DateTime? DueAtUtc { get; init; }

    /// <summary>Number of days before DueAtUtc that reminders should fire.</summary>
    public int ReminderLeadDays { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public Guid CreatedBy { get; init; }

    /// <summary>Optimistic-concurrency token. Must be echoed back on subsequent mutating commands.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for submitting a new generic RegulatoryCalendar obligation (POST v1/compliance/regulatorycalendar). Mirrors <c>CreateRegulatoryCalendarRequest</c>.</summary>
public sealed class CreateRegulatoryCalendarRecordRequest
{
    /// <summary>Gets or sets the optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Gets or sets the human-readable title of the regulatory event/obligation.</summary>
    [Required]
    [StringLength(400)]
    public string EventTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the governing regulatory body (e.g., "OSHA", "EPA", "State DOL").</summary>
    [Required]
    [StringLength(400)]
    public string RegulatoryBody { get; set; } = string.Empty;

    /// <summary>Gets or sets the jurisdiction code the obligation applies to (e.g., "US-CA", "US-FED").</summary>
    [Required]
    [StringLength(50)]
    public string JurisdictionCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the compliance category (e.g., Reporting, Inspection, Renewal, Filing).</summary>
    [Required]
    [StringLength(200)]
    public string ComplianceCategory { get; set; } = string.Empty;

    /// <summary>Gets or sets the identity of the party accountable for meeting this obligation.</summary>
    [Required]
    public Guid ResponsiblePartyId { get; set; }

    /// <summary>Gets or sets the UTC timestamp the obligation is due, if known.</summary>
    public DateTime? DueAtUtc { get; set; }

    /// <summary>Gets or sets the number of days before DueAtUtc that reminders should fire. Defaults to 30.</summary>
    [Range(0, 3650)]
    public int ReminderLeadDays { get; set; } = 30;
}

/// <summary>Result of submitting a new generic RegulatoryCalendar obligation. Mirrors <c>CreateRegulatoryCalendarResult</c>.</summary>
public sealed record CreateRegulatoryCalendarResultDto(Guid RegulatoryCalendarId, string Status);

/// <summary>Result payload shared by the RegulatoryCalendar lifecycle-transition commands (Activate/RecordCompliance/Close/Archive). Mirrors <c>RegulatoryCalendarStatusResult</c>.</summary>
public sealed record RegulatoryCalendarStatusResultDto(Guid RegulatoryCalendarId, string Status);

/// <summary>Request body carrying only an optimistic-concurrency ETag (activate/record-compliance/close/archive).</summary>
public sealed class RegulatoryCalendarEtagRequest
{
    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}
