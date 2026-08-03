// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for SetFeatureFlagRolloutPercentageCommand. Mirrors
//   SetFeatureFlagEnabledCommandHandler's identity resolution and logging conventions.
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

/// <summary>Handles <see cref="SetFeatureFlagRolloutPercentageCommand"/>.</summary>
public sealed class SetFeatureFlagRolloutPercentageCommandHandler
    : IRequestHandler<SetFeatureFlagRolloutPercentageCommand, Result<FeatureFlagDto>>
{
    private const string SystemIdentity = "System";

    private readonly IFeatureFlagRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SetFeatureFlagRolloutPercentageCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SetFeatureFlagRolloutPercentageCommandHandler(
        IFeatureFlagRepository repository,
        ICurrentUserService currentUserService,
        ILogger<SetFeatureFlagRolloutPercentageCommandHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FeatureFlagDto>> Handle(SetFeatureFlagRolloutPercentageCommand request, CancellationToken cancellationToken)
    {
        if (request.RolloutPercentage is < 0 or > 100)
        {
            return Result<FeatureFlagDto>.Failure(new ApplicationError(
                "Operations.Configuration.InvalidRolloutPercentage",
                "RolloutPercentage must be between 0 and 100, or omitted to clear it.",
                Guid.NewGuid()));
        }

        var updatedBy = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId.ToString()
            : SystemIdentity;

        try
        {
            var result = await _repository.SetRolloutPercentageAsync(
                request.DatabaseKey, request.FlagName, request.RolloutPercentage, updatedBy, cancellationToken);

            _logger.LogInformation(
                "Feature flag {FlagName} in database {DatabaseKey} rollout percentage set to {RolloutPercentage} by {UpdatedBy}.",
                request.FlagName, request.DatabaseKey, request.RolloutPercentage?.ToString() ?? "null (cleared)", updatedBy);

            return Result<FeatureFlagDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to set feature flag {FlagName} in database {DatabaseKey} rollout percentage to {RolloutPercentage} (requested by {UpdatedBy}).",
                request.FlagName, request.DatabaseKey, request.RolloutPercentage, updatedBy);

            return Result<FeatureFlagDto>.Failure(new ApplicationError(
                "Operations.Configuration.RolloutPercentageUpdateFailed",
                "Unable to update the feature flag's rollout percentage at this time.",
                Guid.NewGuid()));
        }
    }
}
