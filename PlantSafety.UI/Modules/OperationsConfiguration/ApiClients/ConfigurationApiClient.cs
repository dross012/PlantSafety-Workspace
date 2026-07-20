// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Typed API client for the Operations.Configuration module. Wraps HTTP
//   communication for PlantSafety.Api.Operations.Configuration.Controllers.V1
//   .OperationsConfigurationV1Controller (feature-flag editor only — see
//   ConfigurationDashboardPage.razor for the "Thresholds/Routing Rules/Tenant Overrides
//   out of scope" note).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.OperationsConfiguration.Models;

namespace PlantSafety.UI.Modules.OperationsConfiguration.ApiClients;

/// <summary>Typed API client for the Operations.Configuration module.</summary>
public sealed class ConfigurationApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="ConfigurationApiClient"/>.</summary>
    public ConfigurationApiClient(HttpClient http, ILogger<ConfigurationApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves every feature flag across every known database (ArchiveDB and IntegrationDB).</summary>
    public Task<ApiResponseEnvelope<IReadOnlyList<FeatureFlagDto>>> GetAllFeatureFlagsAsync(
        CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<FeatureFlagDto>>("v1/operations/configuration/feature-flags", cancellationToken);

    /// <summary>Toggles a single feature flag's IsEnabled value in the given database.</summary>
    public Task<ApiResponseEnvelope<FeatureFlagDto>> SetFeatureFlagEnabledAsync(
        string databaseKey,
        string flagName,
        bool isEnabled,
        CancellationToken cancellationToken = default)
        => PutAsync<object, FeatureFlagDto>(
            $"v1/operations/configuration/{databaseKey}/feature-flags/{flagName}",
            new { IsEnabled = isEnabled },
            cancellationToken);
}
