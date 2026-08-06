// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Policies & Procedures module.
//   Wraps HTTP communication for the Safety Policies and SOPs read-model
//   list endpoints. All methods return ApiResponseEnvelope<T>. No HttpClient
//   calls are permitted outside this class for the Policies module.
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
using PlantSafety.UI.Modules.Policies.Models;

namespace PlantSafety.UI.Modules.Policies.ApiClients;

/// <summary>
/// Typed API client for the Policies &amp; Procedures module. Components MUST
/// inject this class and call its methods rather than constructing HTTP
/// requests directly.
/// </summary>
public sealed class PoliciesApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="PoliciesApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public PoliciesApiClient(HttpClient http, ILogger<PoliciesApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves Safety Policies for a tenant within a UTC date range. Backed by GET <c>v1/compliance/policy/safetypolicies</c>.</summary>
    public Task<ApiResponseEnvelope<PoliciesResponseDto>> GetPoliciesAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<PoliciesResponseDto>(
            BuildQuery("v1/compliance/policy/safetypolicies", tenantId, siteId, fromUtc, toUtc),
            cancellationToken);

    /// <summary>Retrieves Standard Operating Procedures for a tenant within a UTC date range. Backed by GET <c>v1/compliance/procedures/sops</c>.</summary>
    public Task<ApiResponseEnvelope<ProceduresResponseDto>> GetProceduresAsync(
        Guid tenantId, Guid? siteId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        => GetAsync<ProceduresResponseDto>(
            BuildQuery("v1/compliance/procedures/sops", tenantId, siteId, fromUtc, toUtc),
            cancellationToken);

    /// <summary>Builds the canonical TenantId/SiteId/From/To query string shared by every Policies read-model endpoint.</summary>
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
    // Compliance.Policy write/lifecycle surface (api/v1/compliance/policy) —
    // Create -> Approve -> RecordAcknowledgment (repeatable) -> Supersede -> Close -> Archive,
    // plus a real by-id GET. Distinct generic Policy aggregate from the read-only Safety
    // Policies/SOPs read-models above — those have no write commands; this is where
    // CreatePolicyCommand and its lifecycle actions actually live.
    // ============================================================

    /// <summary>Retrieves a single generic Policy record by identity, including its ETag. Backed by GET <c>v1/compliance/policy/{id}</c>.</summary>
    public Task<ApiResponseEnvelope<PolicyRecordItemDto>> GetPolicyRecordAsync(
        Guid policyId, CancellationToken cancellationToken = default)
        => GetAsync<PolicyRecordItemDto>($"v1/compliance/policy/{policyId}", cancellationToken);

    /// <summary>Submits a new generic Policy document via the real CreatePolicyCommand.</summary>
    public Task<ApiResponseEnvelope<CreatePolicyResultDto>> CreatePolicyRecordAsync(
        CreatePolicyRecordRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreatePolicyRecordRequest, CreatePolicyResultDto>("v1/compliance/policy", request, cancellationToken);

    /// <summary>Approves a Policy and makes it effective (Created -&gt; Active) via ApprovePolicyCommand.</summary>
    public Task<ApiResponseEnvelope<PolicyStatusResultDto>> ApprovePolicyRecordAsync(
        Guid policyId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, PolicyStatusResultDto>(
            $"v1/compliance/policy/{policyId}/approve", new EtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Records an acknowledgment of an active Policy by the caller via RecordPolicyAcknowledgmentCommand.</summary>
    public Task<ApiResponseEnvelope<PolicyStatusResultDto>> AcknowledgePolicyRecordAsync(
        Guid policyId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, PolicyStatusResultDto>(
            $"v1/compliance/policy/{policyId}/acknowledge", new EtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Supersedes an active Policy with a newer revision (Active -&gt; Completed) via SupersedePolicyCommand.</summary>
    public Task<ApiResponseEnvelope<PolicyStatusResultDto>> SupersedePolicyRecordAsync(
        Guid policyId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, PolicyStatusResultDto>(
            $"v1/compliance/policy/{policyId}/supersede", new EtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Formally closes a superseded Policy (Completed -&gt; Closed) via ClosePolicyCommand.</summary>
    public Task<ApiResponseEnvelope<PolicyStatusResultDto>> ClosePolicyRecordAsync(
        Guid policyId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, PolicyStatusResultDto>(
            $"v1/compliance/policy/{policyId}/close", new EtagRequest { ETag = eTag }, cancellationToken);

    /// <summary>Archives a closed Policy for compliance retention (Closed -&gt; Archived) via ArchivePolicyCommand. Used as this module's soft-delete/retire action.</summary>
    public Task<ApiResponseEnvelope<PolicyStatusResultDto>> ArchivePolicyRecordAsync(
        Guid policyId, string? eTag, CancellationToken cancellationToken = default)
        => PostAsync<EtagRequest, PolicyStatusResultDto>(
            $"v1/compliance/policy/{policyId}/archive", new EtagRequest { ETag = eTag }, cancellationToken);
}
