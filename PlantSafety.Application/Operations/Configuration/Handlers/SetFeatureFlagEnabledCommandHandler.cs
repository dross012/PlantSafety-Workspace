// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for SetFeatureFlagEnabledCommand. Resolves the caller's
//   identity via ICurrentUserService (rather than trusting the SQL login / SYSTEM_USER)
//   and records the toggle at Information (enable) or Warning (disable) severity, since
//   disabling an ops kill-switch is operationally more significant than enabling one.
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

/// <summary>Handles <see cref="SetFeatureFlagEnabledCommand"/>.</summary>
public sealed class SetFeatureFlagEnabledCommandHandler : IRequestHandler<SetFeatureFlagEnabledCommand, Result<FeatureFlagDto>>
{
    private const string SystemIdentity = "System";

    private readonly IFeatureFlagRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SetFeatureFlagEnabledCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SetFeatureFlagEnabledCommandHandler(
        IFeatureFlagRepository repository,
        ICurrentUserService currentUserService,
        ILogger<SetFeatureFlagEnabledCommandHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FeatureFlagDto>> Handle(SetFeatureFlagEnabledCommand request, CancellationToken cancellationToken)
    {
        var updatedBy = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId.ToString()
            : SystemIdentity;

        try
        {
            var result = await _repository.SetEnabledAsync(
                request.DatabaseKey, request.FlagName, request.IsEnabled, updatedBy, cancellationToken);

            if (request.IsEnabled)
            {
                _logger.LogInformation(
                    "Feature flag {FlagName} in database {DatabaseKey} was ENABLED by {UpdatedBy}.",
                    request.FlagName, request.DatabaseKey, updatedBy);
            }
            else
            {
                _logger.LogWarning(
                    "Feature flag {FlagName} in database {DatabaseKey} was DISABLED by {UpdatedBy}.",
                    request.FlagName, request.DatabaseKey, updatedBy);
            }

            return Result<FeatureFlagDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to set feature flag {FlagName} in database {DatabaseKey} to IsEnabled={IsEnabled} (requested by {UpdatedBy}).",
                request.FlagName, request.DatabaseKey, request.IsEnabled, updatedBy);

            return Result<FeatureFlagDto>.Failure(new ApplicationError(
                "Operations.Configuration.ToggleFailed",
                "Unable to update the feature flag at this time.",
                Guid.NewGuid()));
        }
    }
}
