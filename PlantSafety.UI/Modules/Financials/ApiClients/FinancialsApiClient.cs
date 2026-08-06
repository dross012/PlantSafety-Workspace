// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Financials and ROI modules
//   (both are distinct UI surfaces over the same Financials bounded
//   context — mirroring the established
//   Modules/PlanningProjects/ApiClients/PlanningProjectsApiClient.cs
//   precedent of one shared client per bounded context family). Wraps HTTP
//   communication for the real Features/Financials/Budgets CRUD-query
//   endpoints (List/GetById, true server-side pagination) plus a
//   best-inferable ROI endpoint. All methods return
//   ApiResponseEnvelope<T>. No HttpClient calls are permitted outside this
//   class for the Financials or ROI modules.
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
using PlantSafety.UI.Modules.Financials.Models;
using PlantSafety.UI.Modules.ROI.Models;

namespace PlantSafety.UI.Modules.Financials.ApiClients;

/// <summary>
/// Typed API client shared by the Financials and ROI modules. Components MUST
/// inject this class and call its methods rather than constructing HTTP
/// requests directly.
/// </summary>
public sealed class FinancialsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="FinancialsApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public FinancialsApiClient(HttpClient http, ILogger<FinancialsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>
    /// Retrieves a paginated list of Safety Budgets for the authenticated tenant.
    /// Backed by GET <c>v1/features/financials/budgets</c>.
    /// </summary>
    public Task<ApiResponseEnvelope<BudgetListResponseDto>> ListBudgetsAsync(
        int pageNumber = 1,
        int pageSize = 25,
        int? fiscalYear = null,
        Guid? siteId = null,
        string? category = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = $"PageNumber={pageNumber}&PageSize={pageSize}";

        if (fiscalYear.HasValue) query += $"&FiscalYear={fiscalYear.Value}";
        if (siteId.HasValue) query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";
        if (!string.IsNullOrWhiteSpace(category)) query += $"&Category={Uri.EscapeDataString(category)}";
        if (!string.IsNullOrWhiteSpace(status)) query += $"&Status={Uri.EscapeDataString(status)}";

        return GetAsync<BudgetListResponseDto>($"v1/features/financials/budgets?{query}", cancellationToken);
    }

    /// <summary>
    /// Retrieves a single Safety Budget by identifier.
    /// Backed by GET <c>v1/features/financials/budgets/{budgetId}</c>.
    /// </summary>
    public Task<ApiResponseEnvelope<BudgetDto>> GetBudgetByIdAsync(
        Guid budgetId,
        CancellationToken cancellationToken = default)
        => GetAsync<BudgetDto>($"v1/features/financials/budgets/{budgetId}", cancellationToken);

    /// <summary>
    /// Retrieves the computed Safety ROI for a tenant/site/fiscal year. Backed by the
    /// best-inferable route <c>v1/features/financials/roi</c> — no controller currently
    /// exists for this endpoint (see remarks on <see cref="SafetyRoiDto"/>), so callers
    /// MUST treat a Problem/Fault envelope here as "needs backend wiring", not a hard error.
    /// </summary>
    public Task<ApiResponseEnvelope<SafetyRoiDto>> GetRoiAsync(
        Guid tenantId,
        Guid? siteId,
        int fiscalYear,
        CancellationToken cancellationToken = default)
    {
        var query = $"TenantId={Uri.EscapeDataString(tenantId.ToString("D", CultureInfo.InvariantCulture))}&FiscalYear={fiscalYear}";

        if (siteId.HasValue)
        {
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";
        }

        return GetAsync<SafetyRoiDto>($"v1/features/financials/roi?{query}", cancellationToken);
    }

    /// <summary>
    /// Creates a new Safety Budget in Draft status. Backed by
    /// POST <c>v1/features/financials/budgets</c> (requires the Write safety tier policy).
    /// </summary>
    public Task<ApiResponseEnvelope<CreateBudgetResponse>> CreateBudgetAsync(
        CreateBudgetRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<CreateBudgetRequest, CreateBudgetResponse>(
            "v1/features/financials/budgets", request, cancellationToken);

    /// <summary>
    /// Records a new Safety ROI calculation. Backed by
    /// POST <c>v1/features/financials/roi</c> (requires the Write safety tier policy).
    /// Each call creates a new calculation record — there is no update endpoint.
    /// </summary>
    public Task<ApiResponseEnvelope<CalculateRoiResponse>> CalculateRoiAsync(
        CalculateRoiRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<CalculateRoiRequest, CalculateRoiResponse>(
            "v1/features/financials/roi", request, cancellationToken);

    /// <summary>
    /// Creates a new FinancialRecord (Status=Draft) — the "costs" input surface, a distinct
    /// aggregate from Budget. Backed by POST <c>v1/financial-records</c>
    /// (FinancialRecordV1Controller — note the different route root, NOT
    /// v1/features/financials/..., requires the Write safety tier policy).
    /// </summary>
    public Task<ApiResponseEnvelope<CreateFinancialRecordResult>> CreateFinancialRecordAsync(
        CreateFinancialRecordRequest request,
        CancellationToken cancellationToken = default)
        => PostAsync<CreateFinancialRecordRequest, CreateFinancialRecordResult>(
            "v1/financial-records", request, cancellationToken);
}
