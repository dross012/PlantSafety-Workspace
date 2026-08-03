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

using System;
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

    /// <summary>Sets (or clears) a single feature flag's canary-rollout percentage.</summary>
    public Task<ApiResponseEnvelope<FeatureFlagDto>> SetFeatureFlagRolloutPercentageAsync(
        string databaseKey, string flagName, int? rolloutPercentage, CancellationToken cancellationToken = default)
        => PutAsync<object, FeatureFlagDto>(
            $"v1/operations/configuration/{databaseKey}/feature-flags/{flagName}/rollout-percentage",
            new { RolloutPercentage = rolloutPercentage },
            cancellationToken);

    /// <summary>Creates or updates a tenant-level feature flag override.</summary>
    public Task<ApiResponseEnvelope<TenantFeatureFlagOverrideDto>> SetTenantFeatureFlagOverrideAsync(
        string flagName, Guid tenantId, bool enabled, int? rolloutPercentage, CancellationToken cancellationToken = default)
        => PutAsync<object, TenantFeatureFlagOverrideDto>(
            $"v1/operations/configuration/feature-flags/{flagName}/tenant-overrides/{tenantId}",
            new { Enabled = enabled, RolloutPercentage = rolloutPercentage },
            cancellationToken);

    /// <summary>Creates or updates a user-level feature flag override.</summary>
    public Task<ApiResponseEnvelope<UserFeatureFlagOverrideDto>> SetUserFeatureFlagOverrideAsync(
        string flagName, Guid userId, bool enabled, CancellationToken cancellationToken = default)
        => PutAsync<object, UserFeatureFlagOverrideDto>(
            $"v1/operations/configuration/feature-flags/{flagName}/user-overrides/{userId}",
            new { Enabled = enabled },
            cancellationToken);

    /// <summary>Creates or updates a region-level feature flag override.</summary>
    public Task<ApiResponseEnvelope<RegionFeatureFlagOverrideDto>> SetRegionFeatureFlagOverrideAsync(
        string flagName, string region, bool enabled, int? rolloutPercentage, CancellationToken cancellationToken = default)
        => PutAsync<object, RegionFeatureFlagOverrideDto>(
            $"v1/operations/configuration/feature-flags/{flagName}/region-overrides/{Uri.EscapeDataString(region)}",
            new { Enabled = enabled, RolloutPercentage = rolloutPercentage },
            cancellationToken);

    /// <summary>Resolves a feature flag's final on/off outcome across all 4 precedence levels for the given evaluation context.</summary>
    public Task<ApiResponseEnvelope<FeatureFlagResolutionResultDto>> ResolveFeatureFlagAsync(
        string databaseKey, string flagName, Guid? tenantId, Guid? userId, string? region, CancellationToken cancellationToken = default)
    {
        var query = $"tenantId={tenantId}&userId={userId}&region={Uri.EscapeDataString(region ?? string.Empty)}";
        return GetAsync<FeatureFlagResolutionResultDto>(
            $"v1/operations/configuration/{databaseKey}/feature-flags/{flagName}/resolve?{query}", cancellationToken);
    }

    /// <summary>Assigns a tenant's release channel (Stable, Beta, or Canary).</summary>
    public Task<ApiResponseEnvelope<TenantReleaseChannelDto>> SetTenantReleaseChannelAsync(
        Guid tenantId, string channel, CancellationToken cancellationToken = default)
        => PutAsync<object, TenantReleaseChannelDto>(
            $"v1/operations/configuration/tenants/{tenantId}/release-channel",
            new { Channel = channel },
            cancellationToken);

    /// <summary>Executes a real, code-level feature-flag rollback for a bad release.</summary>
    public Task<ApiResponseEnvelope<RollbackResultDto>> TriggerRollbackAsync(
        string databaseKey, string flagName, string reason, bool hardDisable, CancellationToken cancellationToken = default)
        => PostAsync<object, RollbackResultDto>(
            $"v1/operations/configuration/{databaseKey}/feature-flags/{flagName}/rollback",
            new { Reason = reason, HardDisable = hardDisable },
            cancellationToken);
}
