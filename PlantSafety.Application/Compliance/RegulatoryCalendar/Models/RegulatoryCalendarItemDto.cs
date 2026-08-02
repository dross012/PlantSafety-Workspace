// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Application
//  File:           RegulatoryCalendarItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single RegulatoryCalendar record. Backs both
//                  GetRegulatoryCalendarByIdQuery and GetRegulatoryCalendarQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Models;

/// <summary>A single RegulatoryCalendar read-model record.</summary>
public sealed record RegulatoryCalendarItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid RegulatoryCalendarId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Human-readable title of the regulatory event/obligation.</summary>
    public required string EventTitle { get; init; }

    /// <summary>Governing regulatory body (e.g., "OSHA", "EPA", "State DOL").</summary>
    public required string RegulatoryBody { get; init; }

    /// <summary>Jurisdiction code the obligation applies to (e.g., "US-CA", "US-FED").</summary>
    public required string JurisdictionCode { get; init; }

    /// <summary>Compliance category (e.g., Reporting, Inspection, Renewal, Filing).</summary>
    public required string ComplianceCategory { get; init; }

    /// <summary>Identity of the party accountable for meeting this obligation.</summary>
    public Guid? ResponsiblePartyId { get; init; }

    /// <summary>UTC timestamp the obligation is due.</summary>
    public DateTime? DueAtUtc { get; init; }

    /// <summary>Number of days before DueAtUtc that reminders should fire.</summary>
    public int ReminderLeadDays { get; init; }

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
