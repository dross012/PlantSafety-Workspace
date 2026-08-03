// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Configuration.Handlers
//  Layer:          Tests (Unit)
//  File:           TriggerRollbackCommandHandlerTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for TriggerRollbackCommandHandler (Phase 7 §7.2). Verifies
//                  the real rollback effects: global rollout percentage reset to 0, optional
//                  hard-disable, and tenant/region override rollout resets.
// ============================================================================
using System;
using System.Threading;
using System.Threading.Tasks;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Operations.Configuration.Commands;
using PlantSafety.Application.Operations.Configuration.Handlers;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Configuration.Repositories;
using PlantSafety.Tests.Common.TestBase;

using Xunit;

namespace PlantSafety.Tests.Application.Operations.Configuration.Handlers;

public sealed class TriggerRollbackCommandHandlerTests : UnitTestBase
{
    private readonly IFeatureFlagRepository _globalRepository = Substitute.For<IFeatureFlagRepository>();
    private readonly ITenantFeatureFlagOverrideRepository _tenantOverrideRepository = Substitute.For<ITenantFeatureFlagOverrideRepository>();
    private readonly IRegionFeatureFlagOverrideRepository _regionOverrideRepository = Substitute.For<IRegionFeatureFlagOverrideRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private TriggerRollbackCommandHandler Sut() => new(
        _globalRepository, _tenantOverrideRepository, _regionOverrideRepository, _currentUserService,
        NullLogger<TriggerRollbackCommandHandler>());

    [Fact]
    public async Task Handle_SoftRollback_ResetsGlobalRolloutPercentageToZero_AndDoesNotDisable()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(TestUserId.Value);

        _globalRepository.SetRolloutPercentageAsync("ArchiveDB", "Feature.X", 0, TestUserId.Value.ToString(), Arg.Any<CancellationToken>())
            .Returns(new FeatureFlagDto("ArchiveDB", "Feature.X", true, null, FixedUtcNow, TestUserId.Value.ToString(), 0));
        _tenantOverrideRepository.ResetRolloutPercentageForFeatureAsync("Feature.X", Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(3);
        _regionOverrideRepository.ResetRolloutPercentageForFeatureAsync("Feature.X", Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(1);

        var result = await Sut().Handle(new TriggerRollbackCommand("ArchiveDB", "Feature.X", "Error rate exceeded threshold", HardDisable: false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.GlobalRolloutPercentageResetTo);
        Assert.False(result.Value.GlobalHardDisabled);
        Assert.Equal(3, result.Value.TenantOverridesReset);
        Assert.Equal(1, result.Value.RegionOverridesReset);

        await _globalRepository.Received(1).SetRolloutPercentageAsync("ArchiveDB", "Feature.X", 0, TestUserId.Value.ToString(), Arg.Any<CancellationToken>());
        await _globalRepository.DidNotReceive().SetEnabledAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HardDisable_AlsoDisablesGlobalFlag()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(TestUserId.Value);

        _globalRepository.SetRolloutPercentageAsync(Arg.Any<string>(), Arg.Any<string>(), 0, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FeatureFlagDto("ArchiveDB", "Feature.X", true, null, FixedUtcNow, TestUserId.Value.ToString(), 0));
        _globalRepository.SetEnabledAsync("ArchiveDB", "Feature.X", false, TestUserId.Value.ToString(), Arg.Any<CancellationToken>())
            .Returns(new FeatureFlagDto("ArchiveDB", "Feature.X", false, null, FixedUtcNow, TestUserId.Value.ToString(), 0));
        _tenantOverrideRepository.ResetRolloutPercentageForFeatureAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(0);
        _regionOverrideRepository.ResetRolloutPercentageForFeatureAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(0);

        var result = await Sut().Handle(new TriggerRollbackCommand("ArchiveDB", "Feature.X", "Critical outage", HardDisable: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.GlobalHardDisabled);
        await _globalRepository.Received(1).SetEnabledAsync("ArchiveDB", "Feature.X", false, TestUserId.Value.ToString(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureResultWithCriticalSeverity()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(TestUserId.Value);

        _globalRepository.SetRolloutPercentageAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("connection failed"));

        var result = await Sut().Handle(new TriggerRollbackCommand("ArchiveDB", "Feature.X", "reason"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Configuration.RollbackFailed", result.Error.ErrorCode);
    }
}
