// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Typed API client for the Admin Portal's Tenant Management pages. Wraps all
//   HTTP communication for PlantSafety.Api.Tenancy.Controllers.V1.TenantV1Controller
//   (provision/get/list/suspend/reactivate/delete/archive/export).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Admin.Models;

namespace PlantSafety.UI.Modules.Admin.ApiClients;

/// <summary>Typed API client for the Admin Portal's Tenant Management pages.</summary>
public sealed class TenantAdminApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="TenantAdminApiClient"/>.</summary>
    public TenantAdminApiClient(HttpClient http, ILogger<TenantAdminApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Provisions a new platform tenant end-to-end.</summary>
    public Task<ApiResponseEnvelope<TenantProvisioningResultDto>> ProvisionAsync(
        ProvisionTenantRequest request, CancellationToken cancellationToken = default)
        => PostAsync<ProvisionTenantRequest, TenantProvisioningResultDto>("v1/tenancy/tenants", request, cancellationToken);

    /// <summary>Retrieves a single tenant by identity.</summary>
    public Task<ApiResponseEnvelope<TenantAdminDto>> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => GetAsync<TenantAdminDto>($"v1/tenancy/tenants/{tenantId}", cancellationToken);

    /// <summary>Lists every registered tenant, most recently created first.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<TenantAdminDto>>> ListAsync(
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<TenantAdminDto>>("v1/tenancy/tenants", cancellationToken);

    /// <summary>Suspends an Active tenant.</summary>
    public Task<ApiResponseEnvelope<TenantAdminDto>> SuspendAsync(
        Guid tenantId, SuspendTenantRequest request, CancellationToken cancellationToken = default)
        => PostAsync<SuspendTenantRequest, TenantAdminDto>($"v1/tenancy/tenants/{tenantId}/suspend", request, cancellationToken);

    /// <summary>Reactivates a Suspended tenant back to Active.</summary>
    public Task<ApiResponseEnvelope<TenantAdminDto>> ReactivateAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => PostAsync<object, TenantAdminDto>($"v1/tenancy/tenants/{tenantId}/reactivate", new { }, cancellationToken);

    /// <summary>Soft-deletes a tenant.</summary>
    public Task<ApiResponseEnvelope<TenantAdminDto>> DeleteAsync(
        Guid tenantId, DeleteTenantRequest request, CancellationToken cancellationToken = default)
        => DeleteWithBodyAsync(tenantId, request, cancellationToken);

    /// <summary>Archives a tenant's data, transitioning it to Archived status.</summary>
    public Task<ApiResponseEnvelope<TenantAdminDto>> ArchiveAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => PostAsync<object, TenantAdminDto>($"v1/tenancy/tenants/{tenantId}/archive", new { }, cancellationToken);

    /// <summary>Exports a tenant's own canonical Tenancy-context record as a downloadable JSON artifact.</summary>
    public Task<ApiResponseEnvelope<TenantDataExportResultDto>> ExportAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => PostAsync<object, TenantDataExportResultDto>($"v1/tenancy/tenants/{tenantId}/export", new { }, cancellationToken);

    /// <summary>
    /// Issues the soft-delete DELETE request with a JSON body. <see cref="SafetyApiClientBase"/>'s
    /// canonical DeleteAsync helper takes no request body, and DELETE-with-body is this one
    /// endpoint's only such case in the Admin Portal, so it is built directly here rather than
    /// widening the shared base class for a single caller.
    /// </summary>
    private async Task<ApiResponseEnvelope<TenantAdminDto>> DeleteWithBodyAsync(
        Guid tenantId, DeleteTenantRequest request, CancellationToken cancellationToken)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"v1/tenancy/tenants/{tenantId}")
        {
            Content = JsonContent.Create(request),
        };

        try
        {
            var response = await Http.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var wire = await response.Content
                    .ReadFromJsonAsync<ApiEnvelopeWireDto>(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                return ApiResponseEnvelope<TenantAdminDto>.Success(wire!.Data!);
            }

            var problem = await response.Content
                .ReadFromJsonAsync<ProblemDetailsDto>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return ApiResponseEnvelope<TenantAdminDto>.FromProblem(problem ?? ProblemDetailsDto.Unknown((int)response.StatusCode));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ApiResponseEnvelope<TenantAdminDto>.Fault(ex.GetType().Name, ex.Message);
        }
    }

    private sealed class ApiEnvelopeWireDto
    {
        public bool Success { get; init; }
        public TenantAdminDto? Data { get; init; }
    }
}
