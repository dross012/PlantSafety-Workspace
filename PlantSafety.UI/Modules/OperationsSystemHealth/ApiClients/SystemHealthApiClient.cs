// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Typed API client for the Operations.SystemHealth module. Wraps HTTP
//   communication for PlantSafety.Api.Operations.SystemHealth.Controllers.V1
//   .OperationsSystemHealthV1Controller. READ-ONLY — the only method here is a GET.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.OperationsSystemHealth.Models;

namespace PlantSafety.UI.Modules.OperationsSystemHealth.ApiClients;

/// <summary>Typed API client for the Operations.SystemHealth module.</summary>
public sealed class SystemHealthApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="SystemHealthApiClient"/>.</summary>
    public SystemHealthApiClient(HttpClient http, ILogger<SystemHealthApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves the unified System Health rollup report.</summary>
    public Task<ApiResponseEnvelope<SystemHealthReportDto>> GetReportAsync(
        CancellationToken cancellationToken = default)
        => GetAsync<SystemHealthReportDto>("v1/operations/system-health/report", cancellationToken);
}
