// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Typed API client for the Operations.Archival module. Wraps HTTP
//   communication for PlantSafety.Api.Operations.Archival.Controllers.V1
//   .OperationsArchivalV1Controller.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.OperationsArchival.Models;

namespace PlantSafety.UI.Modules.OperationsArchival.ApiClients;

/// <summary>Typed API client for the Operations.Archival module.</summary>
public sealed class ArchivalApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="ArchivalApiClient"/>.</summary>
    public ArchivalApiClient(HttpClient http, ILogger<ArchivalApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves static metadata for the 5 live purge targets.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<PurgeTargetDto>>> GetPurgeTargetsAsync(
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<PurgeTargetDto>>("v1/operations/archival/purge/targets", cancellationToken);

    /// <summary>
    /// Triggers an on-demand retention purge for a single target, or all 5 targets when
    /// <paramref name="target"/> is null. Synchronous -- the response reflects the real,
    /// already-logged Completed/Blocked/Failed outcome of each procedure run.
    /// </summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<PurgeTriggerResultDto>>> TriggerPurgeAsync(
        PurgeTarget? target,
        CancellationToken cancellationToken = default)
        => PostAsync<object, IReadOnlyList<PurgeTriggerResultDto>>(
            "v1/operations/archival/purge/trigger",
            new { Target = target },
            cancellationToken);

    /// <summary>Retrieves recent purge run history, newest first.</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<ArchivalJobDto>>> GetPurgeStatusAsync(
        string? zone = null,
        int top = 50,
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<ArchivalJobDto>>(
            $"v1/operations/archival/purge/status?top={top}" + (string.IsNullOrWhiteSpace(zone) ? string.Empty : $"&zone={Uri.EscapeDataString(zone)}"),
            cancellationToken);

    /// <summary>Retrieves the current retention windows (read-only).</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<TierPolicyDto>>> GetRetentionSettingsAsync(
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<TierPolicyDto>>("v1/operations/archival/retention-settings", cancellationToken);
}
