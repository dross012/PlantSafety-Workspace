// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Regulatory Calendar module.
//   Wraps HTTP communication for the nine Deadlines/Inspections/Renewals
//   read-model list endpoints. All methods return ApiResponseEnvelope<T>.
//   No HttpClient calls are permitted outside this class for the Regulatory
//   Calendar module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.RegulatoryCalendar.Models;

namespace PlantSafety.UI.Modules.RegulatoryCalendar.ApiClients;

/// <summary>
/// Typed API client for the Regulatory Calendar module. Components MUST inject
/// this class and call its methods rather than constructing HTTP requests directly.
/// </summary>
public sealed class RegulatoryCalendarApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="RegulatoryCalendarApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public RegulatoryCalendarApiClient(HttpClient http, ILogger<RegulatoryCalendarApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves Annual Audit deadlines. Backed by GET <c>v1/compliance/regulatory-calendar/deadlines/annual-audit</c>.</summary>
    public Task<ApiResponseEnvelope<RegulatoryCalendarResponseDto<AnnualAuditItemDto>>> GetAnnualAuditDeadlinesAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<RegulatoryCalendarResponseDto<AnnualAuditItemDto>>(
            BuildQuery("v1/compliance/regulatory-calendar/deadlines/annual-audit", tenantId, siteId, fromUtc, toUtc), cancellationToken);

    /// <summary>Retrieves EPA deadlines. Backed by GET <c>v1/compliance/regulatory-calendar/deadlines/epa</c>.</summary>
    public Task<ApiResponseEnvelope<RegulatoryCalendarResponseDto<EpaDeadlineItemDto>>> GetEpaDeadlinesAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<RegulatoryCalendarResponseDto<EpaDeadlineItemDto>>(
            BuildQuery("v1/compliance/regulatory-calendar/deadlines/epa", tenantId, siteId, fromUtc, toUtc), cancellationToken);

    /// <summary>Retrieves DOT inspections. Backed by GET <c>v1/compliance/regulatory-calendar/inspections/dot</c>.</summary>
    public Task<ApiResponseEnvelope<RegulatoryCalendarResponseDto<DotInspectionItemDto>>> GetDotInspectionsAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<RegulatoryCalendarResponseDto<DotInspectionItemDto>>(
            BuildQuery("v1/compliance/regulatory-calendar/inspections/dot", tenantId, siteId, fromUtc, toUtc), cancellationToken);

    /// <summary>Retrieves Environmental Sampling inspections. Backed by GET <c>v1/compliance/regulatory-calendar/inspections/environmental-sampling</c>.</summary>
    public Task<ApiResponseEnvelope<RegulatoryCalendarResponseDto<EnvironmentalSamplingItemDto>>> GetEnvironmentalSamplingInspectionsAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<RegulatoryCalendarResponseDto<EnvironmentalSamplingItemDto>>(
            BuildQuery("v1/compliance/regulatory-calendar/inspections/environmental-sampling", tenantId, siteId, fromUtc, toUtc), cancellationToken);

    /// <summary>Retrieves Equipment inspections. Backed by GET <c>v1/compliance/regulatory-calendar/inspections/equipment</c>.</summary>
    public Task<ApiResponseEnvelope<RegulatoryCalendarResponseDto<EquipmentInspectionItemDto>>> GetEquipmentInspectionsAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<RegulatoryCalendarResponseDto<EquipmentInspectionItemDto>>(
            BuildQuery("v1/compliance/regulatory-calendar/inspections/equipment", tenantId, siteId, fromUtc, toUtc), cancellationToken);

    /// <summary>Retrieves OSHA inspections. Backed by GET <c>v1/compliance/regulatory-calendar/inspections/osha</c>.</summary>
    public Task<ApiResponseEnvelope<RegulatoryCalendarResponseDto<OshaInspectionItemDto>>> GetOshaInspectionsAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<RegulatoryCalendarResponseDto<OshaInspectionItemDto>>(
            BuildQuery("v1/compliance/regulatory-calendar/inspections/osha", tenantId, siteId, fromUtc, toUtc), cancellationToken);

    /// <summary>Retrieves DOT credential renewals. Backed by GET <c>v1/compliance/regulatory-calendar/renewals/dot</c>.</summary>
    public Task<ApiResponseEnvelope<RegulatoryCalendarResponseDto<DotRenewalItemDto>>> GetDotRenewalsAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<RegulatoryCalendarResponseDto<DotRenewalItemDto>>(
            BuildQuery("v1/compliance/regulatory-calendar/renewals/dot", tenantId, siteId, fromUtc, toUtc), cancellationToken);

    /// <summary>Retrieves ISO certification renewals. Backed by GET <c>v1/compliance/regulatory-calendar/renewals/iso</c>.</summary>
    public Task<ApiResponseEnvelope<RegulatoryCalendarResponseDto<IsoRenewalItemDto>>> GetIsoRenewalsAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<RegulatoryCalendarResponseDto<IsoRenewalItemDto>>(
            BuildQuery("v1/compliance/regulatory-calendar/renewals/iso", tenantId, siteId, fromUtc, toUtc), cancellationToken);

    /// <summary>Retrieves Training program renewals. Backed by GET <c>v1/compliance/regulatory-calendar/renewals/training</c>.</summary>
    public Task<ApiResponseEnvelope<RegulatoryCalendarResponseDto<TrainingRenewalItemDto>>> GetTrainingRenewalsAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<RegulatoryCalendarResponseDto<TrainingRenewalItemDto>>(
            BuildQuery("v1/compliance/regulatory-calendar/renewals/training", tenantId, siteId, fromUtc, toUtc), cancellationToken);

    /// <summary>Builds the canonical TenantId/SiteId/From/To query string shared by every Regulatory Calendar read-model endpoint.</summary>
    private static string BuildQuery(string path, Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc)
    {
        var query = $"TenantId={Uri.EscapeDataString(tenantId.ToString("D", CultureInfo.InvariantCulture))}" +
                    $"&From={Uri.EscapeDataString(fromUtc.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&To={Uri.EscapeDataString(toUtc.ToString("O", CultureInfo.InvariantCulture))}";

        if (siteId.HasValue)
        {
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";
        }

        return $"{path}?{query}";
    }

    // ============================================================
    // Compliance.RegulatoryCalendar write/lifecycle surface (api/v1/compliance/regulatorycalendar) —
    // Create -> Activate -> RecordCompliance -> Close -> Archive, plus a real by-id GET. Distinct
    // generic RegulatoryCalendar aggregate from the 9 read-only Deadlines/Inspections/Renewals
    // source verticals above — those have no write commands; this is where
    // CreateRegulatoryCalendarCommand and its lifecycle actions actually live.
    // ============================================================

    /// <summary>Retrieves a single generic RegulatoryCalendar record by identity, including its ETag. Backed by GET <c>v1/compliance/regulatorycalendar/{id}</c>.</summary>
    public Task<ApiResponseEnvelope<RegulatoryCalendarItemDto>> GetRegulatoryCalendarRecordAsync(
        Guid regulatoryCalendarId, CancellationToken cancellationToken = default)
        => GetAsync<RegulatoryCalendarItemDto>($"v1/compliance/regulatorycalendar/{regulatoryCalendarId}", cancellationToken);

    /// <summary>Submits a new generic RegulatoryCalendar obligation via the real CreateRegulatoryCalendarCommand.</summary>
    public Task<ApiResponseEnvelope<CreateRegulatoryCalendarResultDto>> CreateRegulatoryCalendarRecordAsync(
        CreateRegulatoryCalendarRecordRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateRegulatoryCalendarRecordRequest, CreateRegulatoryCalendarResultDto>(
            "v1/compliance/regulatorycalendar", request, cancellationToken);

    /// <summary>Activates a RegulatoryCalendar obligation (Created -&gt; Active) via ActivateRegulatoryCalendarCommand.</summary>
    public Task<ApiResponseEnvelope<RegulatoryCalendarStatusResultDto>> ActivateRegulatoryCalendarRecordAsync(
        Guid regulatoryCalendarId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<RegulatoryCalendarEtagRequest, RegulatoryCalendarStatusResultDto>(
            $"v1/compliance/regulatorycalendar/{regulatoryCalendarId}/activate", new RegulatoryCalendarEtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Records compliance with an obligation (Active -&gt; Completed) via RecordRegulatoryCalendarComplianceCommand.</summary>
    public Task<ApiResponseEnvelope<RegulatoryCalendarStatusResultDto>> RecordRegulatoryCalendarComplianceAsync(
        Guid regulatoryCalendarId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<RegulatoryCalendarEtagRequest, RegulatoryCalendarStatusResultDto>(
            $"v1/compliance/regulatorycalendar/{regulatoryCalendarId}/record-compliance", new RegulatoryCalendarEtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Formally closes a completed obligation (Completed -&gt; Closed) via CloseRegulatoryCalendarCommand.</summary>
    public Task<ApiResponseEnvelope<RegulatoryCalendarStatusResultDto>> CloseRegulatoryCalendarRecordAsync(
        Guid regulatoryCalendarId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<RegulatoryCalendarEtagRequest, RegulatoryCalendarStatusResultDto>(
            $"v1/compliance/regulatorycalendar/{regulatoryCalendarId}/close", new RegulatoryCalendarEtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Archives a closed obligation for compliance retention (Closed -&gt; Archived) via ArchiveRegulatoryCalendarCommand. Used as this module's soft-delete/retire action.</summary>
    public Task<ApiResponseEnvelope<RegulatoryCalendarStatusResultDto>> ArchiveRegulatoryCalendarRecordAsync(
        Guid regulatoryCalendarId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<RegulatoryCalendarEtagRequest, RegulatoryCalendarStatusResultDto>(
            $"v1/compliance/regulatorycalendar/{regulatoryCalendarId}/archive", new RegulatoryCalendarEtagRequest { ETag = eTag }, cancellationToken);
}
