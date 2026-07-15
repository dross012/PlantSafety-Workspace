// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-15
// FILE DESCRIPTION: Typed API client for the Process Management module. Wraps
//   the real Features/ProcessManagement/Processes CRUD-query endpoints
//   (List/GetById, true server-side pagination). No HttpClient calls are
//   permitted outside this class for the Process Management module.
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
using PlantSafety.UI.Modules.ProcessManagement.Models;

namespace PlantSafety.UI.Modules.ProcessManagement.ApiClients;

/// <summary>Typed API client for the Process Management module.</summary>
public sealed class ProcessManagementApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="ProcessManagementApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public ProcessManagementApiClient(HttpClient http, ILogger<ProcessManagementApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>
    /// Retrieves a paginated list of Process Definitions for the authenticated tenant.
    /// Backed by GET <c>v1/features/process-management/processes</c>.
    /// </summary>
    public Task<ApiResponseEnvelope<ProcessListResponseDto>> ListProcessesAsync(
        int pageNumber = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
        => GetAsync<ProcessListResponseDto>(
            $"v1/features/process-management/processes?PageNumber={pageNumber}&PageSize={pageSize}",
            cancellationToken);

    /// <summary>
    /// Retrieves a single Process Definition by identifier.
    /// Backed by GET <c>v1/features/process-management/processes/{processId}</c>.
    /// </summary>
    public Task<ApiResponseEnvelope<ProcessDefinitionDto>> GetProcessByIdAsync(
        Guid processId,
        CancellationToken cancellationToken = default)
        => GetAsync<ProcessDefinitionDto>($"v1/features/process-management/processes/{processId}", cancellationToken);
}
