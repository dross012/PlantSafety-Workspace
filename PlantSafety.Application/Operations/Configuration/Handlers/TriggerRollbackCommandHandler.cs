// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for TriggerRollbackCommand. Executes a real, code-level
//   feature-flag rollback: resets the global flag's rollout percentage to 0 (and optionally
//   hard-disables it), then resets every tenant/region override's rollout percentage to 0
//   for the same feature. Logged at Warning/Critical severity since a rollback is an
//   operationally significant event.
// SchemaVersion: 1

using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Configuration.Commands;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Configuration.Repositories;

namespace PlantSafety.Application.Operations.Configuration.Handlers;

/// <summary>Handles <see cref="TriggerRollbackCommand"/>.</summary>
public sealed class TriggerRollbackCommandHandler : IRequestHandler<TriggerRollbackCommand, Result<RollbackResultDto>>
{
    private const string SystemIdentity = "System";

    private readonly IFeatureFlagRepository _globalRepository;
    private readonly ITenantFeatureFlagOverrideRepository _tenantOverrideRepository;
    private readonly IRegionFeatureFlagOverrideRepository _regionOverrideRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TriggerRollbackCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public TriggerRollbackCommandHandler(
        IFeatureFlagRepository globalRepository,
        ITenantFeatureFlagOverrideRepository tenantOverrideRepository,
        IRegionFeatureFlagOverrideRepository regionOverrideRepository,
        ICurrentUserService currentUserService,
        ILogger<TriggerRollbackCommandHandler> logger)
    {
        _globalRepository = globalRepository;
        _tenantOverrideRepository = tenantOverrideRepository;
        _regionOverrideRepository = regionOverrideRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RollbackResultDto>> Handle(TriggerRollbackCommand request, CancellationToken cancellationToken)
    {
        var triggeredBy = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId.ToString()
            : SystemIdentity;

        try
        {
            // 1. Revert the global flag's rollout percentage to 0.
            var globalFlag = await _globalRepository.SetRolloutPercentageAsync(
                request.DatabaseKey, request.FeatureName, 0, triggeredBy, cancellationToken);

            // 2. Optionally hard-disable the global flag outright.
            if (request.HardDisable)
            {
                globalFlag = await _globalRepository.SetEnabledAsync(
                    request.DatabaseKey, request.FeatureName, false, triggeredBy, cancellationToken);
            }

            // 3. Reset every tenant-level override's rollout percentage to 0 for this feature.
            var tenantOverridesReset = await _tenantOverrideRepository.ResetRolloutPercentageForFeatureAsync(
                request.FeatureName, triggeredBy, cancellationToken);

            // 4. Reset every region-level override's rollout percentage to 0 for this feature.
            var regionOverridesReset = await _regionOverrideRepository.ResetRolloutPercentageForFeatureAsync(
                request.FeatureName, triggeredBy, cancellationToken);

            var result = new RollbackResultDto(
                request.FeatureName,
                request.DatabaseKey,
                GlobalRolloutPercentageResetTo: 0,
                GlobalHardDisabled: request.HardDisable,
                TenantOverridesReset: tenantOverridesReset,
                RegionOverridesReset: regionOverridesReset,
                request.Reason,
                DateTime.UtcNow,
                triggeredBy);

            _logger.LogCritical(
                "ROLLBACK TRIGGERED for feature {FeatureName} in database {DatabaseKey} by {TriggeredBy}. Reason: {Reason}. " +
                "GlobalRolloutPercentage reset to 0 (HardDisable={HardDisable}); {TenantOverridesReset} tenant override(s) and {RegionOverridesReset} region override(s) reset.",
                request.FeatureName, request.DatabaseKey, triggeredBy, request.Reason, request.HardDisable, tenantOverridesReset, regionOverridesReset);

            return Result<RollbackResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to trigger rollback for feature {FeatureName} in database {DatabaseKey} (requested by {TriggeredBy}).",
                request.FeatureName, request.DatabaseKey, triggeredBy);

            return Result<RollbackResultDto>.Failure(new ApplicationError(
                "Operations.Configuration.RollbackFailed",
                "Unable to trigger the feature flag rollback at this time.",
                Guid.NewGuid(),
                ErrorSeverity.Critical));
        }
    }
}
