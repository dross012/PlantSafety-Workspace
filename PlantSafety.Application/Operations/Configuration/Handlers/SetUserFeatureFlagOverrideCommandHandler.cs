// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for SetUserFeatureFlagOverrideCommand. Resolves the
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

/// <summary>Handles <see cref="SetUserFeatureFlagOverrideCommand"/>.</summary>
public sealed class SetUserFeatureFlagOverrideCommandHandler : IRequestHandler<SetUserFeatureFlagOverrideCommand, Result<UserFeatureFlagOverrideDto>>
{
    private const string SystemIdentity = "System";

    private readonly IUserFeatureFlagOverrideRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SetUserFeatureFlagOverrideCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SetUserFeatureFlagOverrideCommandHandler(
        IUserFeatureFlagOverrideRepository repository,
        ICurrentUserService currentUserService,
        ILogger<SetUserFeatureFlagOverrideCommandHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<UserFeatureFlagOverrideDto>> Handle(SetUserFeatureFlagOverrideCommand request, CancellationToken cancellationToken)
    {
        var updatedBy = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId.ToString()
            : SystemIdentity;

        try
        {
            var result = await _repository.SetAsync(
                request.OverrideUserId, request.FeatureName, request.Enabled, updatedBy, cancellationToken);

            _logger.LogInformation(
                "User feature flag override for {FeatureName}/user {OverrideUserId} set to Enabled={Enabled} by {UpdatedBy}.",
                request.FeatureName, request.OverrideUserId, request.Enabled, updatedBy);

            return Result<UserFeatureFlagOverrideDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to set user feature flag override for {FeatureName}/user {OverrideUserId} (requested by {UpdatedBy}).",
                request.FeatureName, request.OverrideUserId, updatedBy);

            return Result<UserFeatureFlagOverrideDto>.Failure(new ApplicationError(
                "Operations.Configuration.UserOverrideFailed",
                "Unable to update the user feature flag override at this time.",
                Guid.NewGuid()));
        }
    }
}
