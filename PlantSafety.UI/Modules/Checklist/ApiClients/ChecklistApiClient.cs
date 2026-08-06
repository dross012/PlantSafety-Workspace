// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Checklist module. Wraps HTTP
//   communication for the Checklists read-model list endpoint. All methods
//   return ApiResponseEnvelope<T>. No HttpClient calls are permitted
//   outside this class for the Checklist module.
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
using PlantSafety.UI.Modules.Checklist.Models;

namespace PlantSafety.UI.Modules.Checklist.ApiClients;

/// <summary>
/// Typed API client for the Checklist module. Components MUST inject this
/// class and call its methods rather than constructing HTTP requests directly.
/// </summary>
public sealed class ChecklistApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="ChecklistApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public ChecklistApiClient(HttpClient http, ILogger<ChecklistApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves Checklists for a tenant within a UTC date range. Backed by GET <c>v1/operational/checklists</c>.</summary>
    public Task<ApiResponseEnvelope<ChecklistsResponseDto>> GetChecklistsAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var query = $"TenantId={Uri.EscapeDataString(tenantId.ToString("D", CultureInfo.InvariantCulture))}" +
                    $"&From={Uri.EscapeDataString(fromUtc.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&To={Uri.EscapeDataString(toUtc.ToString("O", CultureInfo.InvariantCulture))}";

        if (siteId.HasValue)
        {
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";
        }

        return GetAsync<ChecklistsResponseDto>($"v1/operational/checklists?{query}", cancellationToken);
    }

    // ============================================================
    // Checklists write/lifecycle surface (api/v1/checklists/templates) —
    // Create -> AddItem -> Publish -> Retire. Distinct write-side ChecklistTemplate
    // aggregate from the read-only "v1/operational/checklists" list-row projection above
    // (which has no matching Create/Update commands of its own). No GetById query exists
    // for ChecklistTemplate, so the UI tracks the created/updated record purely from each
    // command's own response, mirroring the Admin Portal's TenantProvisionPage pattern.
    // ============================================================

    /// <summary>Creates a new ChecklistTemplate via the real CreateChecklistTemplateCommand.</summary>
    public Task<ApiResponseEnvelope<CreateChecklistTemplateResultDto>> CreateChecklistTemplateAsync(
        CreateChecklistTemplateRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateChecklistTemplateRequest, CreateChecklistTemplateResultDto>(
            "v1/checklists/templates", request, cancellationToken);

    /// <summary>Adds an inspection item to a draft ChecklistTemplate via AddChecklistTemplateItemCommand.</summary>
    public Task<ApiResponseEnvelope<AddChecklistTemplateItemResultDto>> AddChecklistTemplateItemAsync(
        Guid checklistTemplateId, AddChecklistTemplateItemRequest request, CancellationToken cancellationToken = default)
        => PostAsync<AddChecklistTemplateItemRequest, AddChecklistTemplateItemResultDto>(
            $"v1/checklists/templates/{checklistTemplateId}/items", request, cancellationToken);

    /// <summary>Publishes a draft ChecklistTemplate (Created -&gt; Active) via PublishChecklistTemplateCommand.</summary>
    public Task<ApiResponseEnvelope<PublishChecklistTemplateResultDto>> PublishChecklistTemplateAsync(
        Guid checklistTemplateId, CancellationToken cancellationToken = default)
        => PostAsync<object, PublishChecklistTemplateResultDto>(
            $"v1/checklists/templates/{checklistTemplateId}/publish", new { }, cancellationToken);

    /// <summary>Retires a ChecklistTemplate via RetireChecklistTemplateCommand. Used as this module's soft-delete/retire action.</summary>
    public Task<ApiResponseEnvelope<RetireChecklistTemplateResultDto>> RetireChecklistTemplateAsync(
        Guid checklistTemplateId, RetireChecklistTemplateRequest request, CancellationToken cancellationToken = default)
        => PostAsync<RetireChecklistTemplateRequest, RetireChecklistTemplateResultDto>(
            $"v1/checklists/templates/{checklistTemplateId}/retire", request, cancellationToken);
}
