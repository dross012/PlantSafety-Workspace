// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Emergency Management module. Wraps
//   HTTP communication for activating response plans (POST) and for the
//   List/GetById read endpoints under /v1/features/safety-events/emergency-responses,
//   backed by PlantSafety.Api.Features.SafetyEvents.EmergencyManagement
//   .Controllers.V1.EmergencyResponsesV1Controller.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.EmergencyManagement.Models;

namespace PlantSafety.UI.Modules.EmergencyManagement.ApiClients;

/// <summary>Typed API client for the Emergency Management module.</summary>
public sealed class EmergencyManagementApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="EmergencyManagementApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public EmergencyManagementApiClient(HttpClient http, ILogger<EmergencyManagementApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a paginated list of emergency responses.</summary>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="pageSize">Maximum items per page.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<EmergencyResponseDto>>> ListEmergencyResponsesAsync(
        int pageNumber = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<EmergencyResponseDto>>(
            $"v1/features/safety-events/emergency-responses?pageNumber={pageNumber}&pageSize={pageSize}",
            cancellationToken);

    /// <summary>Retrieves a single emergency response by its identifier.</summary>
    /// <param name="emergencyResponseId">The unique emergency response identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<EmergencyResponseDto>> GetEmergencyResponseAsync(
        Guid emergencyResponseId,
        CancellationToken cancellationToken = default)
        => GetAsync<EmergencyResponseDto>($"v1/features/safety-events/emergency-responses/{emergencyResponseId}", cancellationToken);
}
