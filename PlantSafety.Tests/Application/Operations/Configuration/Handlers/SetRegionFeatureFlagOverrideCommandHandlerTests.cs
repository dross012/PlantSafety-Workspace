// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Configuration.Handlers
//  Layer:          Tests (Unit)
//  File:           SetRegionFeatureFlagOverrideCommandHandlerTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SetRegionFeatureFlagOverrideCommandHandler.
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

public sealed class SetRegionFeatureFlagOverrideCommandHandlerTests : UnitTestBase
{
    private readonly IRegionFeatureFlagOverrideRepository _repository = Substitute.For<IRegionFeatureFlagOverrideRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private SetRegionFeatureFlagOverrideCommandHandler Sut() =>
        new(_repository, _currentUserService, NullLogger<SetRegionFeatureFlagOverrideCommandHandler>());

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldPassCurrentUserIdAsUpdatedBy()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(TestUserId.Value);

        var expected = new RegionFeatureFlagOverrideDto("EU", "Intelligence.NewDashboard", true, 60, FixedUtcNow, TestUserId.Value.ToString());
        _repository.SetAsync("EU", "Intelligence.NewDashboard", true, 60, TestUserId.Value.ToString(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await Sut().Handle(new SetRegionFeatureFlagOverrideCommand("EU", "Intelligence.NewDashboard", true, 60), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldFallBackToSystemIdentity()
    {
        _currentUserService.IsAuthenticated.Returns(false);

        _repository.SetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new RegionFeatureFlagOverrideDto("APAC", "Feature.X", false, null, FixedUtcNow, "System"));

        await Sut().Handle(new SetRegionFeatureFlagOverrideCommand("APAC", "Feature.X", false, null), CancellationToken.None);

        await _repository.Received(1).SetAsync("APAC", "Feature.X", false, null, "System", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(TestUserId.Value);

        _repository.SetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("connection failed"));

        var result = await Sut().Handle(new SetRegionFeatureFlagOverrideCommand("US", "Feature.X", true, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Configuration.RegionOverrideFailed", result.Error.ErrorCode);
    }
}
