// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Configuration.Handlers
//  Layer:          Tests (Unit)
//  File:           SetFeatureFlagRolloutPercentageCommandHandlerTests.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SetFeatureFlagRolloutPercentageCommandHandler (Phase 6.6,
//                  §6.2). Mirrors SetFeatureFlagEnabledCommandHandlerTests's conventions.
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

public sealed class SetFeatureFlagRolloutPercentageCommandHandlerTests : UnitTestBase
{
    private readonly IFeatureFlagRepository _repository = Substitute.For<IFeatureFlagRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private SetFeatureFlagRolloutPercentageCommandHandler Sut() =>
        new(_repository, _currentUserService, NullLogger<SetFeatureFlagRolloutPercentageCommandHandler>());

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldPassCurrentUserIdAsUpdatedBy()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(TestUserId.Value);

        var expected = new FeatureFlagDto("ArchiveDB", "RetentionPurge.Enabled", true, "Ops kill-switch.", FixedUtcNow, TestUserId.Value.ToString(), 25);
        _repository.SetRolloutPercentageAsync("ArchiveDB", "RetentionPurge.Enabled", 25, TestUserId.Value.ToString(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await Sut().Handle(new SetFeatureFlagRolloutPercentageCommand("ArchiveDB", "RetentionPurge.Enabled", 25), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
        await _repository.Received(1).SetRolloutPercentageAsync("ArchiveDB", "RetentionPurge.Enabled", 25, TestUserId.Value.ToString(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task Handle_WhenPercentageOutOfRange_ShouldReturnFailureWithoutCallingRepository(int invalidPercentage)
    {
        var result = await Sut().Handle(new SetFeatureFlagRolloutPercentageCommand("ArchiveDB", "RetentionPurge.Enabled", invalidPercentage), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Configuration.InvalidRolloutPercentage", result.Error.ErrorCode);
        await _repository.DidNotReceive().SetRolloutPercentageAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNullPercentage_ShouldClearRolloutPercentage()
    {
        _currentUserService.IsAuthenticated.Returns(false);

        _repository.SetRolloutPercentageAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FeatureFlagDto("IntegrationDB", "ServiceBroker.ReplayProcessingEnabled", true, null, FixedUtcNow, "System", null));

        var result = await Sut().Handle(new SetFeatureFlagRolloutPercentageCommand("IntegrationDB", "ServiceBroker.ReplayProcessingEnabled", null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _repository.Received(1).SetRolloutPercentageAsync("IntegrationDB", "ServiceBroker.ReplayProcessingEnabled", null, "System", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(TestUserId.Value);

        _repository.SetRolloutPercentageAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("connection failed"));

        var result = await Sut().Handle(new SetFeatureFlagRolloutPercentageCommand("ArchiveDB", "RetentionPurge.Enabled", 50), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Configuration.RolloutPercentageUpdateFailed", result.Error.ErrorCode);
    }
}
