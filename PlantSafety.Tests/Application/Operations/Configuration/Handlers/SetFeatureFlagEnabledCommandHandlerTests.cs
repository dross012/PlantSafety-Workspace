// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Configuration.Handlers
//  Layer:          Tests (Unit)
//  File:           SetFeatureFlagEnabledCommandHandlerTests.cs
//  Created:        2026-07-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SetFeatureFlagEnabledCommandHandler. Verifies the
//                  handler resolves UpdatedBy from ICurrentUserService (never SYSTEM_USER)
//                  and falls back to "System" when unauthenticated.
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

public sealed class SetFeatureFlagEnabledCommandHandlerTests : UnitTestBase
{
    private readonly IFeatureFlagRepository _repository = Substitute.For<IFeatureFlagRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private SetFeatureFlagEnabledCommandHandler Sut() =>
        new(_repository, _currentUserService, NullLogger<SetFeatureFlagEnabledCommandHandler>());

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldPassCurrentUserIdAsUpdatedBy()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(TestUserId.Value);

        var expected = new FeatureFlagDto("ArchiveDB", "RetentionPurge.Enabled", false, "Ops kill-switch.", FixedUtcNow, TestUserId.Value.ToString());
        _repository.SetEnabledAsync("ArchiveDB", "RetentionPurge.Enabled", false, TestUserId.Value.ToString(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await Sut().Handle(new SetFeatureFlagEnabledCommand("ArchiveDB", "RetentionPurge.Enabled", false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
        await _repository.Received(1).SetEnabledAsync("ArchiveDB", "RetentionPurge.Enabled", false, TestUserId.Value.ToString(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldFallBackToSystemIdentity()
    {
        _currentUserService.IsAuthenticated.Returns(false);

        _repository.SetEnabledAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new FeatureFlagDto("IntegrationDB", "ServiceBroker.ReplayProcessingEnabled", true, null, FixedUtcNow, "System"));

        await Sut().Handle(new SetFeatureFlagEnabledCommand("IntegrationDB", "ServiceBroker.ReplayProcessingEnabled", true), CancellationToken.None);

        await _repository.Received(1).SetEnabledAsync("IntegrationDB", "ServiceBroker.ReplayProcessingEnabled", true, "System", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(TestUserId.Value);

        _repository.SetEnabledAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("connection failed"));

        var result = await Sut().Handle(new SetFeatureFlagEnabledCommand("ArchiveDB", "RetentionPurge.Enabled", true), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Configuration.ToggleFailed", result.Error.ErrorCode);
    }
}
