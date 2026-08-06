// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Management of Change module.
//   Wraps HTTP communication for the MOC Workflows/Approvals/RiskAssessments
//   read-model list endpoints, plus the real Create command exposed by the
//   Features/Compliance/ManagementOfChange bounded context
//   (POST v1/features/compliance/management-of-change, CreateMocCommand).
//   All methods return ApiResponseEnvelope<T>. No HttpClient calls are
//   permitted outside this class for the MOC module.
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
using PlantSafety.UI.Modules.MOC.Models;

namespace PlantSafety.UI.Modules.MOC.ApiClients;

/// <summary>
/// Typed API client for the Management of Change module. Components MUST inject
/// this class and call its methods rather than constructing HTTP requests directly.
/// </summary>
public sealed class MocApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="MocApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public MocApiClient(HttpClient http, ILogger<MocApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves MOC workflow instances for a tenant within a UTC date range. Backed by GET <c>v1/compliance/management-of-change/mocworkflows</c>.</summary>
    public Task<ApiResponseEnvelope<MocWorkflowsResponseDto>> GetWorkflowsAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<MocWorkflowsResponseDto>(
            BuildQuery("v1/compliance/management-of-change/mocworkflows", tenantId, siteId, fromUtc, toUtc),
            cancellationToken);

    /// <summary>Retrieves MOC approval decisions for a tenant within a UTC date range. Backed by GET <c>v1/compliance/management-of-change/approvals</c>.</summary>
    public Task<ApiResponseEnvelope<MocApprovalsResponseDto>> GetApprovalsAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<MocApprovalsResponseDto>(
            BuildQuery("v1/compliance/management-of-change/approvals", tenantId, siteId, fromUtc, toUtc),
            cancellationToken);

    /// <summary>Retrieves MOC risk assessments for a tenant within a UTC date range. Backed by GET <c>v1/compliance/management-of-change/riskassessments</c>.</summary>
    public Task<ApiResponseEnvelope<MocRiskAssessmentsResponseDto>> GetRiskAssessmentsAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<MocRiskAssessmentsResponseDto>(
            BuildQuery("v1/compliance/management-of-change/riskassessments", tenantId, siteId, fromUtc, toUtc),
            cancellationToken);

    /// <summary>
    /// Initiates a new Management of Change request in Draft status. Backed by
    /// POST <c>v1/features/compliance/management-of-change</c> (CreateMocCommand).
    /// </summary>
    /// <param name="request">The MOC fields to submit.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the new MOC request's identifier or a problem/fault.</returns>
    public Task<ApiResponseEnvelope<Guid>> CreateMocAsync(
        Models.CreateMocRequest request, CancellationToken cancellationToken = default)
        => PostAsync<Models.CreateMocRequest, Guid>(
            "v1/features/compliance/management-of-change", request, cancellationToken);

    /// <summary>Builds the canonical TenantId/SiteId/From/To query string shared by every MOC read-model endpoint.</summary>
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
}
