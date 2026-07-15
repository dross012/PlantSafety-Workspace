// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: UI-layer mapping service projecting each of the nine
//   Regulatory Calendar source DTOs into the unified CalendarEntryViewModel
//   shape. Named and shaped after the platform's standard Mapster
//   DTO -> ViewModel convention (Adapt-style pure static projections, no
//   side effects); implemented as hand-written static methods rather than
//   the Mapster package (see
//   Modules/PlanningProjects/Mappings/PlanningProjectsMappingService.cs for
//   the precedent establishing this convention).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.RegulatoryCalendar.Models;

namespace PlantSafety.UI.Modules.RegulatoryCalendar.Mappings;

/// <summary>Pure, stateless DTO -&gt; ViewModel projections for the Regulatory Calendar module.</summary>
public static class RegulatoryCalendarMappingService
{
    /// <summary>Projects Annual Audit deadline items into unified calendar entries.</summary>
    public static IEnumerable<CalendarEntryViewModel> ToEntries(this IEnumerable<AnnualAuditItemDto> items) =>
        items.Select(d => new CalendarEntryViewModel
        {
            Source = CalendarEntrySource.AnnualAudit,
            Group = CalendarEntryGroup.Deadline,
            EntryId = d.DeadlineId,
            Label = d.AuditScope,
            DateUtc = d.DueAtUtc,
            DateLabel = "Due",
            Status = d.Status,
            StatusIndicatorState = ToStatusIndicatorState(d.Status),
            ResponsiblePartyId = d.ResponsibleParty,
            TenantId = d.TenantId,
            SiteId = d.SiteId,
        });

    /// <summary>Projects EPA deadline items into unified calendar entries.</summary>
    public static IEnumerable<CalendarEntryViewModel> ToEntries(this IEnumerable<EpaDeadlineItemDto> items) =>
        items.Select(d => new CalendarEntryViewModel
        {
            Source = CalendarEntrySource.Epa,
            Group = CalendarEntryGroup.Deadline,
            EntryId = d.DeadlineId,
            Label = d.EPARegulation,
            DateUtc = d.DueAtUtc,
            DateLabel = "Due",
            Status = d.Status,
            StatusIndicatorState = ToStatusIndicatorState(d.Status),
            ResponsiblePartyId = d.ResponsibleParty,
            TenantId = d.TenantId,
            SiteId = d.SiteId,
        });

    /// <summary>Projects DOT inspection items into unified calendar entries.</summary>
    public static IEnumerable<CalendarEntryViewModel> ToEntries(this IEnumerable<DotInspectionItemDto> items) =>
        items.Select(d => new CalendarEntryViewModel
        {
            Source = CalendarEntrySource.DotInspection,
            Group = CalendarEntryGroup.Inspection,
            EntryId = d.InspectionId,
            Label = d.InspectionScope,
            DateUtc = d.ScheduledAtUtc,
            DateLabel = "Scheduled",
            Status = d.Status,
            StatusIndicatorState = ToStatusIndicatorState(d.Status),
            ResponsiblePartyId = d.Inspector,
            TenantId = d.TenantId,
            SiteId = d.SiteId,
        });

    /// <summary>Projects Environmental Sampling inspection items into unified calendar entries.</summary>
    public static IEnumerable<CalendarEntryViewModel> ToEntries(this IEnumerable<EnvironmentalSamplingItemDto> items) =>
        items.Select(d => new CalendarEntryViewModel
        {
            Source = CalendarEntrySource.EnvironmentalSampling,
            Group = CalendarEntryGroup.Inspection,
            EntryId = d.InspectionId,
            Label = d.SampleType,
            DateUtc = d.ScheduledAtUtc,
            DateLabel = "Scheduled",
            Status = d.Status,
            StatusIndicatorState = ToStatusIndicatorState(d.Status),
            ResponsiblePartyId = d.Inspector,
            TenantId = d.TenantId,
            SiteId = d.SiteId,
        });

    /// <summary>Projects Equipment inspection items into unified calendar entries.</summary>
    public static IEnumerable<CalendarEntryViewModel> ToEntries(this IEnumerable<EquipmentInspectionItemDto> items) =>
        items.Select(d => new CalendarEntryViewModel
        {
            Source = CalendarEntrySource.EquipmentInspection,
            Group = CalendarEntryGroup.Inspection,
            EntryId = d.InspectionId,
            Label = d.EquipmentCategory,
            DateUtc = d.ScheduledAtUtc,
            DateLabel = "Scheduled",
            Status = d.Status,
            StatusIndicatorState = ToStatusIndicatorState(d.Status),
            ResponsiblePartyId = d.Inspector,
            TenantId = d.TenantId,
            SiteId = d.SiteId,
        });

    /// <summary>Projects OSHA inspection items into unified calendar entries.</summary>
    public static IEnumerable<CalendarEntryViewModel> ToEntries(this IEnumerable<OshaInspectionItemDto> items) =>
        items.Select(d => new CalendarEntryViewModel
        {
            Source = CalendarEntrySource.OshaInspection,
            Group = CalendarEntryGroup.Inspection,
            EntryId = d.InspectionId,
            Label = d.InspectionScope,
            DateUtc = d.ScheduledAtUtc,
            DateLabel = "Scheduled",
            Status = d.Status,
            StatusIndicatorState = ToStatusIndicatorState(d.Status),
            ResponsiblePartyId = d.Inspector,
            TenantId = d.TenantId,
            SiteId = d.SiteId,
        });

    /// <summary>Projects DOT credential renewal items into unified calendar entries.</summary>
    public static IEnumerable<CalendarEntryViewModel> ToEntries(this IEnumerable<DotRenewalItemDto> items) =>
        items.Select(d => new CalendarEntryViewModel
        {
            Source = CalendarEntrySource.DotRenewal,
            Group = CalendarEntryGroup.Renewal,
            EntryId = d.RenewalId,
            Label = d.CredentialType,
            DateUtc = d.ExpiresAtUtc,
            DateLabel = "Expires",
            Status = d.Status,
            StatusIndicatorState = ToStatusIndicatorState(d.Status),
            ResponsiblePartyId = d.ResponsibleParty,
            TenantId = d.TenantId,
            SiteId = d.SiteId,
        });

    /// <summary>Projects ISO certification renewal items into unified calendar entries.</summary>
    public static IEnumerable<CalendarEntryViewModel> ToEntries(this IEnumerable<IsoRenewalItemDto> items) =>
        items.Select(d => new CalendarEntryViewModel
        {
            Source = CalendarEntrySource.IsoRenewal,
            Group = CalendarEntryGroup.Renewal,
            EntryId = d.RenewalId,
            Label = d.CertificationStandard,
            DateUtc = d.ExpiresAtUtc,
            DateLabel = "Expires",
            Status = d.Status,
            StatusIndicatorState = ToStatusIndicatorState(d.Status),
            ResponsiblePartyId = d.ResponsibleParty,
            TenantId = d.TenantId,
            SiteId = d.SiteId,
        });

    /// <summary>Projects Training program renewal items into unified calendar entries.</summary>
    public static IEnumerable<CalendarEntryViewModel> ToEntries(this IEnumerable<TrainingRenewalItemDto> items) =>
        items.Select(d => new CalendarEntryViewModel
        {
            Source = CalendarEntrySource.TrainingRenewal,
            Group = CalendarEntryGroup.Renewal,
            EntryId = d.RenewalId,
            Label = d.TrainingProgram,
            DateUtc = d.ExpiresAtUtc,
            DateLabel = "Expires",
            Status = d.Status,
            StatusIndicatorState = ToStatusIndicatorState(d.Status),
            ResponsiblePartyId = d.ResponsibleParty,
            TenantId = d.TenantId,
            SiteId = d.SiteId,
        });

    /// <summary>Maps a raw Regulatory Calendar status code to the closest StatusIndicatorComponent operational state.</summary>
    /// <param name="status">The raw status code (case-insensitive; covers Deadline/Inspection/Renewal vocabularies).</param>
    public static string ToStatusIndicatorState(string status) => status.Trim().ToLowerInvariant() switch
    {
        "scheduled" or "completed" or "current" or "passed" => "Healthy",
        "pending" or "upcoming" or "dueforrenewal" => "Warning",
        "overdue" or "expired" or "failed" or "missed" => "Critical",
        "cancelled" => "Offline",
        _ => "Offline",
    };
}
