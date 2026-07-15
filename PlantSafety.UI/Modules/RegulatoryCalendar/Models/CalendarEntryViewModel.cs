// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Unified calendar-entry view model. All nine Regulatory
//   Calendar source types (Deadlines/Inspections/Renewals) are projected
//   into this single shape so one calendar-style DataGridComponent and one
//   detail page can serve all of them.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.RegulatoryCalendar.Models;

/// <summary>The top-level Regulatory Calendar grouping a source type belongs to.</summary>
public enum CalendarEntryGroup
{
    /// <summary>A fixed compliance deadline.</summary>
    Deadline,

    /// <summary>A scheduled inspection.</summary>
    Inspection,

    /// <summary>A credential/certification/training renewal.</summary>
    Renewal,
}

/// <summary>The specific Regulatory Calendar source endpoint a row/detail record was projected from.</summary>
public enum CalendarEntrySource
{
    /// <summary>Deadlines/AnnualAudit.</summary>
    AnnualAudit,
    /// <summary>Deadlines/EPA.</summary>
    Epa,
    /// <summary>Inspections/DOT.</summary>
    DotInspection,
    /// <summary>Inspections/EnvironmentalSampling.</summary>
    EnvironmentalSampling,
    /// <summary>Inspections/Equipment.</summary>
    EquipmentInspection,
    /// <summary>Inspections/OSHA.</summary>
    OshaInspection,
    /// <summary>Renewals/DOT.</summary>
    DotRenewal,
    /// <summary>Renewals/ISO.</summary>
    IsoRenewal,
    /// <summary>Renewals/Training.</summary>
    TrainingRenewal,
}

/// <summary>Unified calendar entry — one row across all nine Regulatory Calendar source types.</summary>
public sealed class CalendarEntryViewModel
{
    /// <summary>Gets the source endpoint this entry was projected from.</summary>
    public required CalendarEntrySource Source { get; init; }

    /// <summary>Gets the top-level grouping (Deadline/Inspection/Renewal).</summary>
    public required CalendarEntryGroup Group { get; init; }

    /// <summary>Gets the record identifier (DeadlineId/InspectionId/RenewalId).</summary>
    public required Guid EntryId { get; init; }

    /// <summary>Gets the primary descriptive label (AuditScope/EPARegulation/InspectionScope/SampleType/EquipmentCategory/CredentialType/CertificationStandard/TrainingProgram).</summary>
    public required string Label { get; init; }

    /// <summary>Gets the relevant date (DueAtUtc/ScheduledAtUtc/ExpiresAtUtc).</summary>
    public required DateTime DateUtc { get; init; }

    /// <summary>Gets a label describing what <see cref="DateUtc"/> represents (Due/Scheduled/Expires).</summary>
    public required string DateLabel { get; init; }

    /// <summary>Gets the raw status code.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the StatusIndicatorComponent operational state derived from <see cref="Status"/>.</summary>
    public required string StatusIndicatorState { get; init; }

    /// <summary>Gets the identifier of the responsible party or assigned inspector.</summary>
    public required Guid ResponsiblePartyId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }
}
