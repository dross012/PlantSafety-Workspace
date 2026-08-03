// ============================================================================
// File: OperationsConfigurationV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-20
// Description: V1 controller surfacing the real Phase 14 Governance
//   [System].[FeatureFlag] tables (144-feature-flags.sql), confirmed to exist ONLY in
//   PlantSafety.ArchiveDB and PlantSafety.IntegrationDB. Routes all requests through
//   MediatR ISender to the Application layer. No domain logic or data access here.
//
//   SCOPE NOTE (2026-07-20, superseded 2026-08-03 for Tenant/User/Region Overrides only):
//   originally, "Thresholds", "Routing Rules", and "Tenant Overrides" had NO backing SQL
//   Server tables in this codebase. Phase 7 (SaaS Productization Blueprint §7.1/§7.2, migration
//   200-tenant-user-region-feature-flag-overrides.sql) added real tables + endpoints for
//   Tenant/User/Region feature flag overrides, release channels, and rollback automation
//   below. "Thresholds" and "Routing Rules" remain out of scope (still no backing tables).
// ============================================================================
using PlantSafety.Api.Operations.Configuration.Contracts.Requests;
using PlantSafety.Application.Operations.Configuration.Commands;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Configuration.Queries;

namespace PlantSafety.Api.Operations.Configuration.Controllers.V1;

/// <summary>V1 controller for the Operations.Configuration module (feature-flag editor).</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operations/configuration")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Operations.Configuration")]
public sealed class OperationsConfigurationV1Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="OperationsConfigurationV1Controller"/>.</summary>
    public OperationsConfigurationV1Controller(ISender sender) => _sender = sender;

    private string CorrelationId =>
        HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString() ?? string.Empty;

    /// <summary>
    /// Returns every feature flag. When <paramref name="databaseKey"/> is omitted, returns
    /// flags from every known database (currently ArchiveDB and IntegrationDB) concatenated.
    /// </summary>
    [HttpGet("feature-flags")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FeatureFlagDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FeatureFlagDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFeatureFlagsAsync([FromQuery] string? databaseKey, CancellationToken cancellationToken)
    {
        var result = string.IsNullOrWhiteSpace(databaseKey)
            ? await _sender.Send(new GetAllFeatureFlagsQuery(), cancellationToken)
            : await _sender.Send(new GetFeatureFlagsQuery(databaseKey), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<FeatureFlagDto>>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<IReadOnlyList<FeatureFlagDto>>.Ok(result.Value, CorrelationId));
    }

    /// <summary>
    /// Toggles a single feature flag's IsEnabled value in the given database. A normal,
    /// reversible UPDATE. Uses PUT (not PATCH) to match the existing SafetyApiClientBase
    /// UI infrastructure, which provides GetAsync/PostAsync/PutAsync helpers only.
    /// </summary>
    [HttpPut("{databaseKey}/feature-flags/{flagName}")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<FeatureFlagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FeatureFlagDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetFeatureFlagEnabledAsync(
        [FromRoute] string databaseKey,
        [FromRoute] string flagName,
        [FromBody] SetFeatureFlagEnabledRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SetFeatureFlagEnabledCommand(databaseKey, flagName, request.IsEnabled), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<FeatureFlagDto>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<FeatureFlagDto>.Ok(result.Value, CorrelationId));
    }

    /// <summary>
    /// Sets (or clears) a single feature flag's canary-rollout percentage (Phase 6.6, §6.2
    /// Blue/Green + Canary + Tenant Rollout). Combine with IFeatureFlagRepository's existing
    /// IsEnabled switch and PlantSafety.Application.Common.Rollout.ITenantRolloutPolicy to
    /// decide per-tenant rollout-wave membership.
    /// </summary>
    [HttpPut("{databaseKey}/feature-flags/{flagName}/rollout-percentage")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<FeatureFlagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FeatureFlagDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetFeatureFlagRolloutPercentageAsync(
        [FromRoute] string databaseKey,
        [FromRoute] string flagName,
        [FromBody] SetFeatureFlagRolloutPercentageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SetFeatureFlagRolloutPercentageCommand(databaseKey, flagName, request.RolloutPercentage), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<FeatureFlagDto>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<FeatureFlagDto>.Ok(result.Value, CorrelationId));
    }

    // ------------------------------------------------------------------
    // Phase 7 §7.1 -- Tenant/User/Region feature flag overrides + resolver
    // ------------------------------------------------------------------

    /// <summary>Creates or updates a tenant-level feature flag override.</summary>
    [HttpPut("feature-flags/{flagName}/tenant-overrides/{tenantId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<TenantFeatureFlagOverrideDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TenantFeatureFlagOverrideDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetTenantFeatureFlagOverrideAsync(
        [FromRoute] string flagName,
        [FromRoute] Guid tenantId,
        [FromBody] SetTenantFeatureFlagOverrideRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SetTenantFeatureFlagOverrideCommand(tenantId, flagName, request.Enabled, request.RolloutPercentage), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<TenantFeatureFlagOverrideDto>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<TenantFeatureFlagOverrideDto>.Ok(result.Value, CorrelationId));
    }

    /// <summary>Creates or updates a user-level feature flag override (e.g. an individual beta tester).</summary>
    [HttpPut("feature-flags/{flagName}/user-overrides/{userId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<UserFeatureFlagOverrideDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserFeatureFlagOverrideDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetUserFeatureFlagOverrideAsync(
        [FromRoute] string flagName,
        [FromRoute] Guid userId,
        [FromBody] SetUserFeatureFlagOverrideRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SetUserFeatureFlagOverrideCommand(userId, flagName, request.Enabled), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<UserFeatureFlagOverrideDto>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<UserFeatureFlagOverrideDto>.Ok(result.Value, CorrelationId));
    }

    /// <summary>Creates or updates a region-level feature flag override (e.g. a feature only live in EU first).</summary>
    [HttpPut("feature-flags/{flagName}/region-overrides/{region}")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<RegionFeatureFlagOverrideDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RegionFeatureFlagOverrideDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetRegionFeatureFlagOverrideAsync(
        [FromRoute] string flagName,
        [FromRoute] string region,
        [FromBody] SetRegionFeatureFlagOverrideRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SetRegionFeatureFlagOverrideCommand(region, flagName, request.Enabled, request.RolloutPercentage), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<RegionFeatureFlagOverrideDto>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<RegionFeatureFlagOverrideDto>.Ok(result.Value, CorrelationId));
    }

    /// <summary>
    /// Resolves a feature flag's final on/off outcome across all 4 precedence levels
    /// (user > tenant > region > global) for the given evaluation context.
    /// </summary>
    [HttpGet("{databaseKey}/feature-flags/{flagName}/resolve")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<FeatureFlagResolutionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FeatureFlagResolutionResult>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResolveFeatureFlagAsync(
        [FromRoute] string databaseKey,
        [FromRoute] string flagName,
        [FromQuery] Guid? tenantId,
        [FromQuery] Guid? userId,
        [FromQuery] string? region,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ResolveFeatureFlagQuery(databaseKey, flagName, tenantId, userId, region), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<FeatureFlagResolutionResult>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<FeatureFlagResolutionResult>.Ok(result.Value, CorrelationId));
    }

    // ------------------------------------------------------------------
    // Phase 7 §7.2 -- Tenant release channels + rollback automation
    // ------------------------------------------------------------------

    /// <summary>Assigns a tenant's release channel (Stable, Beta, or Canary).</summary>
    [HttpPut("tenants/{tenantId:guid}/release-channel")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<TenantReleaseChannelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TenantReleaseChannelDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetTenantReleaseChannelAsync(
        [FromRoute] Guid tenantId,
        [FromBody] SetTenantReleaseChannelRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SetTenantReleaseChannelCommand(tenantId, request.Channel), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<TenantReleaseChannelDto>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<TenantReleaseChannelDto>.Ok(result.Value, CorrelationId));
    }

    /// <summary>
    /// Executes a real, code-level feature-flag rollback for a bad release: reverts the
    /// canary rollout percentage to 0 across the global flag and every tenant/region
    /// override for the given feature (and optionally hard-disables the global flag).
    /// </summary>
    [HttpPost("{databaseKey}/feature-flags/{flagName}/rollback")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<RollbackResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RollbackResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TriggerRollbackAsync(
        [FromRoute] string databaseKey,
        [FromRoute] string flagName,
        [FromBody] TriggerRollbackRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new TriggerRollbackCommand(databaseKey, flagName, request.Reason, request.HardDisable), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<RollbackResultDto>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<RollbackResultDto>.Ok(result.Value, CorrelationId));
    }
}
