// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Safety Programs module. Wraps
//   all HTTP communication for the Safety Programs List and Program Detail
//   pages. All methods return ApiResponseEnvelope<T>.
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
using PlantSafety.UI.Modules.SafetyPrograms.Models;

namespace PlantSafety.UI.Modules.SafetyPrograms.ApiClients;

/// <summary>Typed API client for the Safety Programs module.</summary>
public sealed class SafetyProgramsApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="SafetyProgramsApiClient"/>.</summary>
    public SafetyProgramsApiClient(HttpClient http, ILogger<SafetyProgramsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a paginated page of safety programs.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<SafetyProgramListItemDto>>> GetProgramsAsync(
        int pageNumber = 1, int pageSize = 25, string? statusFilter = null, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<SafetyProgramListItemDto>>(
            $"v1/safety-programs?pageNumber={pageNumber}&pageSize={pageSize}" +
            (string.IsNullOrWhiteSpace(statusFilter) ? string.Empty : $"&status={Uri.EscapeDataString(statusFilter)}"),
            cancellationToken);

    /// <summary>Retrieves the full detail for a single safety program.</summary>
    public Task<ApiResponseEnvelope<SafetyProgramDetailDto>> GetProgramAsync(Guid programId, CancellationToken cancellationToken = default)
        => GetAsync<SafetyProgramDetailDto>($"v1/safety-programs/{programId}", cancellationToken);
}
