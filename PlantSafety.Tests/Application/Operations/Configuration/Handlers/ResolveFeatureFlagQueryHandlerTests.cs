// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Configuration.Handlers
//  Layer:          Tests (Unit)
//  File:           ResolveFeatureFlagQueryHandlerTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ResolveFeatureFlagQueryHandler. Verifies the handler
//                  fetches every precedence level's raw state and delegates composition to
//                  IFeatureFlagResolver with the correct arguments.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PlantSafety.Application.Operations.Configuration.Handlers;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Configuration.Queries;
using PlantSafety.Application.Operations.Configuration.Repositories;
using PlantSafety.Application.Operations.Configuration.Resolution;
using PlantSafety.Tests.Common.TestBase;

using Xunit;

namespace PlantSafety.Tests.Application.Operations.Configuration.Handlers;

public sealed class ResolveFeatureFlagQueryHandlerTests : UnitTestBase
{
    private readonly IFeatureFlagRepository _globalRepository = Substitute.For<IFeatureFlagRepository>();
    private readonly ITenantFeatureFlagOverrideRepository _tenantOverrideRepository = Substitute.For<ITenantFeatureFlagOverrideRepository>();
    private readonly IUserFeatureFlagOverrideRepository _userOverrideRepository = Substitute.For<IUserFeatureFlagOverrideRepository>();
    private readonly IRegionFeatureFlagOverrideRepository _regionOverrideRepository = Substitute.For<IRegionFeatureFlagOverrideRepository>();
    private readonly ITenantReleaseChannelRepository _channelRepository = Substitute.For<ITenantReleaseChannelRepository>();
    private readonly IFeatureFlagResolver _resolver = Substitute.For<IFeatureFlagResolver>();

    private ResolveFeatureFlagQueryHandler Sut() => new(
        _globalRepository, _tenantOverrideRepository, _userOverrideRepository, _regionOverrideRepository, _channelRepository, _resolver,
        NullLogger<ResolveFeatureFlagQueryHandler>());

    [Fact]
    public async Task Handle_FetchesEveryLevel_AndDelegatesToResolver()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const string region = "EU";
        const string feature = "Intelligence.NewDashboard";

        var globalFlag = new FeatureFlagDto("ArchiveDB", feature, true, "desc", FixedUtcNow, "admin", 50);
        _globalRepository.GetAllAsync("ArchiveDB", Arg.Any<CancellationToken>())
            .Returns(new List<FeatureFlagDto> { globalFlag });

        var userOverride = new UserFeatureFlagOverrideDto(userId, feature, true, FixedUtcNow, "admin");
        _userOverrideRepository.FindAsync(userId, feature, Arg.Any<CancellationToken>()).Returns(userOverride);

        var tenantOverride = new TenantFeatureFlagOverrideDto(tenantId, feature, true, null, FixedUtcNow, "admin");
        _tenantOverrideRepository.FindAsync(tenantId, feature, Arg.Any<CancellationToken>()).Returns(tenantOverride);

        var regionOverride = new RegionFeatureFlagOverrideDto(region, feature, true, null, FixedUtcNow, "admin");
        _regionOverrideRepository.FindAsync(region, feature, Arg.Any<CancellationToken>()).Returns(regionOverride);

        var channelDto = new TenantReleaseChannelDto(tenantId, TenantReleaseChannel.Beta, FixedUtcNow, "admin");
        _channelRepository.FindAsync(tenantId, Arg.Any<CancellationToken>()).Returns(channelDto);

        var expectedResolution = new FeatureFlagResolutionResult(feature, true, FeatureFlagSourceLevel.User, null, tenantId, userId, region);
        _resolver.Resolve(
            feature, true, 50, true,
            regionOverride, tenantOverride, userOverride,
            tenantId, userId, region, TenantReleaseChannel.Beta)
            .Returns(expectedResolution);

        var result = await Sut().Handle(new ResolveFeatureFlagQuery("ArchiveDB", feature, tenantId, userId, region), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedResolution, result.Value);
        _resolver.Received(1).Resolve(feature, true, 50, true, regionOverride, tenantOverride, userOverride, tenantId, userId, region, TenantReleaseChannel.Beta);
    }

    [Fact]
    public async Task Handle_NoContextSupplied_SkipsOverrideLookups()
    {
        const string feature = "Feature.X";
        _globalRepository.GetAllAsync("ArchiveDB", Arg.Any<CancellationToken>())
            .Returns(new List<FeatureFlagDto>());

        _resolver.Resolve(feature, false, null, false, null, null, null, null, null, null, null)
            .Returns(new FeatureFlagResolutionResult(feature, false, FeatureFlagSourceLevel.Default, null, null, null, null));

        var result = await Sut().Handle(new ResolveFeatureFlagQuery("ArchiveDB", feature, null, null, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _userOverrideRepository.DidNotReceiveWithAnyArgs().FindAsync(default, default!, default);
        await _tenantOverrideRepository.DidNotReceiveWithAnyArgs().FindAsync(default, default!, default);
        await _regionOverrideRepository.DidNotReceiveWithAnyArgs().FindAsync(default!, default!, default);
        await _channelRepository.DidNotReceiveWithAnyArgs().FindAsync(default, default);
    }

    [Fact]
    public async Task Handle_WhenGlobalRepositoryThrows_ReturnsFailureResult()
    {
        _globalRepository.GetAllAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("connection failed"));

        var result = await Sut().Handle(new ResolveFeatureFlagQuery("ArchiveDB", "Feature.X", null, null, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Configuration.ResolveFailed", result.Error.ErrorCode);
    }
}
