// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for SetTenantReleaseChannelCommand. Resolves the caller's
//   identity via ICurrentUserService (never SYSTEM_USER).
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

/// <summary>Handles <see cref="SetTenantReleaseChannelCommand"/>.</summary>
public sealed class SetTenantReleaseChannelCommandHandler : IRequestHandler<SetTenantReleaseChannelCommand, Result<TenantReleaseChannelDto>>
{
    private const string SystemIdentity = "System";

    private readonly ITenantReleaseChannelRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SetTenantReleaseChannelCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SetTenantReleaseChannelCommandHandler(
        ITenantReleaseChannelRepository repository,
        ICurrentUserService currentUserService,
        ILogger<SetTenantReleaseChannelCommandHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TenantReleaseChannelDto>> Handle(SetTenantReleaseChannelCommand request, CancellationToken cancellationToken)
    {
        var updatedBy = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId.ToString()
            : SystemIdentity;

        try
        {
            var result = await _repository.SetAsync(request.OverrideTenantId, request.Channel, updatedBy, cancellationToken);

            _logger.LogInformation(
                "Tenant {OverrideTenantId} release channel set to {Channel} by {UpdatedBy}.",
                request.OverrideTenantId, request.Channel, updatedBy);

            return Result<TenantReleaseChannelDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to set release channel for tenant {OverrideTenantId} (requested by {UpdatedBy}).",
                request.OverrideTenantId, updatedBy);

            return Result<TenantReleaseChannelDto>.Failure(new ApplicationError(
                "Operations.Configuration.ReleaseChannelFailed",
                "Unable to update the tenant release channel at this time.",
                Guid.NewGuid()));
        }
    }
}
