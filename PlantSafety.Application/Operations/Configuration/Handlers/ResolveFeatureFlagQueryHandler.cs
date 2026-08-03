// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for ResolveFeatureFlagQuery. Fetches the raw state of every
//   precedence level (global flag, region/tenant/user overrides, tenant release channel)
//   and delegates the deterministic composition to IFeatureFlagResolver.
// SchemaVersion: 1

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Configuration.Queries;
using PlantSafety.Application.Operations.Configuration.Repositories;
using PlantSafety.Application.Operations.Configuration.Resolution;

namespace PlantSafety.Application.Operations.Configuration.Handlers;

/// <summary>Handles <see cref="ResolveFeatureFlagQuery"/>.</summary>
public sealed class ResolveFeatureFlagQueryHandler : IRequestHandler<ResolveFeatureFlagQuery, Result<FeatureFlagResolutionResult>>
{
    private readonly IFeatureFlagRepository _globalRepository;
    private readonly ITenantFeatureFlagOverrideRepository _tenantOverrideRepository;
    private readonly IUserFeatureFlagOverrideRepository _userOverrideRepository;
    private readonly IRegionFeatureFlagOverrideRepository _regionOverrideRepository;
    private readonly ITenantReleaseChannelRepository _channelRepository;
    private readonly IFeatureFlagResolver _resolver;
    private readonly ILogger<ResolveFeatureFlagQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ResolveFeatureFlagQueryHandler(
        IFeatureFlagRepository globalRepository,
        ITenantFeatureFlagOverrideRepository tenantOverrideRepository,
        IUserFeatureFlagOverrideRepository userOverrideRepository,
        IRegionFeatureFlagOverrideRepository regionOverrideRepository,
        ITenantReleaseChannelRepository channelRepository,
        IFeatureFlagResolver resolver,
        ILogger<ResolveFeatureFlagQueryHandler> logger)
    {
        _globalRepository = globalRepository;
        _tenantOverrideRepository = tenantOverrideRepository;
        _userOverrideRepository = userOverrideRepository;
        _regionOverrideRepository = regionOverrideRepository;
        _channelRepository = channelRepository;
        _resolver = resolver;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FeatureFlagResolutionResult>> Handle(ResolveFeatureFlagQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var globalFlags = await _globalRepository.GetAllAsync(request.DatabaseKey, cancellationToken);
            var globalFlag = globalFlags.FirstOrDefault(f => f.FlagName == request.FeatureName);

            UserFeatureFlagOverrideDto? userOverride = request.UserId is Guid userId
                ? await _userOverrideRepository.FindAsync(userId, request.FeatureName, cancellationToken)
                : null;

            TenantFeatureFlagOverrideDto? tenantOverride = request.TenantId is Guid tenantId
                ? await _tenantOverrideRepository.FindAsync(tenantId, request.FeatureName, cancellationToken)
                : null;

            RegionFeatureFlagOverrideDto? regionOverride = !string.IsNullOrWhiteSpace(request.Region)
                ? await _regionOverrideRepository.FindAsync(request.Region, request.FeatureName, cancellationToken)
                : null;

            TenantReleaseChannel? channel = null;
            if (request.TenantId is Guid channelTenantId)
            {
                var channelDto = await _channelRepository.FindAsync(channelTenantId, cancellationToken);
                channel = channelDto?.Channel;
            }

            var resolution = _resolver.Resolve(
                request.FeatureName,
                globalFlag?.IsEnabled ?? false,
                globalFlag?.RolloutPercentage,
                globalFlag is not null,
                regionOverride,
                tenantOverride,
                userOverride,
                request.TenantId,
                request.UserId,
                request.Region,
                channel);

            return Result<FeatureFlagResolutionResult>.Success(resolution);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to resolve feature flag {FeatureName} for tenant {TenantId}, user {UserId}, region {Region}.",
                request.FeatureName, request.TenantId, request.UserId, request.Region);

            return Result<FeatureFlagResolutionResult>.Failure(new ApplicationError(
                "Operations.Configuration.ResolveFailed",
                "Unable to resolve the feature flag at this time.",
                Guid.NewGuid()));
        }
    }
}
