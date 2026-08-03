// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for SetRegionFeatureFlagOverrideCommand. Resolves the
//   caller's identity via ICurrentUserService (never SYSTEM_USER).
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

/// <summary>Handles <see cref="SetRegionFeatureFlagOverrideCommand"/>.</summary>
public sealed class SetRegionFeatureFlagOverrideCommandHandler : IRequestHandler<SetRegionFeatureFlagOverrideCommand, Result<RegionFeatureFlagOverrideDto>>
{
    private const string SystemIdentity = "System";

    private readonly IRegionFeatureFlagOverrideRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SetRegionFeatureFlagOverrideCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SetRegionFeatureFlagOverrideCommandHandler(
        IRegionFeatureFlagOverrideRepository repository,
        ICurrentUserService currentUserService,
        ILogger<SetRegionFeatureFlagOverrideCommandHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RegionFeatureFlagOverrideDto>> Handle(SetRegionFeatureFlagOverrideCommand request, CancellationToken cancellationToken)
    {
        var updatedBy = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId.ToString()
            : SystemIdentity;

        try
        {
            var result = await _repository.SetAsync(
                request.Region, request.FeatureName, request.Enabled, request.RolloutPercentage, updatedBy, cancellationToken);

            _logger.LogInformation(
                "Region feature flag override for {FeatureName}/region {Region} set to Enabled={Enabled}, RolloutPercentage={RolloutPercentage} by {UpdatedBy}.",
                request.FeatureName, request.Region, request.Enabled, request.RolloutPercentage, updatedBy);

            return Result<RegionFeatureFlagOverrideDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to set region feature flag override for {FeatureName}/region {Region} (requested by {UpdatedBy}).",
                request.FeatureName, request.Region, updatedBy);

            return Result<RegionFeatureFlagOverrideDto>.Failure(new ApplicationError(
                "Operations.Configuration.RegionOverrideFailed",
                "Unable to update the region feature flag override at this time.",
                Guid.NewGuid()));
        }
    }
}
