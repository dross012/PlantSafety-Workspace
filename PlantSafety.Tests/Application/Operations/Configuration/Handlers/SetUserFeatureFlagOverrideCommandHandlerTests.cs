// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Configuration.Handlers
//  Layer:          Tests (Unit)
//  File:           SetUserFeatureFlagOverrideCommandHandlerTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SetUserFeatureFlagOverrideCommandHandler.
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

public sealed class SetUserFeatureFlagOverrideCommandHandlerTests : UnitTestBase
{
    private readonly IUserFeatureFlagOverrideRepository _repository = Substitute.For<IUserFeatureFlagOverrideRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private SetUserFeatureFlagOverrideCommandHandler Sut() =>
        new(_repository, _currentUserService, NullLogger<SetUserFeatureFlagOverrideCommandHandler>());

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldPassCurrentUserIdAsUpdatedBy()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(TestUserId.Value);

        var targetUserId = Guid.NewGuid();
        var expected = new UserFeatureFlagOverrideDto(targetUserId, "Intelligence.NewDashboard", true, FixedUtcNow, TestUserId.Value.ToString());
        _repository.SetAsync(targetUserId, "Intelligence.NewDashboard", true, TestUserId.Value.ToString(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await Sut().Handle(new SetUserFeatureFlagOverrideCommand(targetUserId, "Intelligence.NewDashboard", true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldFallBackToSystemIdentity()
    {
        _currentUserService.IsAuthenticated.Returns(false);
        var targetUserId = Guid.NewGuid();

        _repository.SetAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new UserFeatureFlagOverrideDto(targetUserId, "Feature.X", false, FixedUtcNow, "System"));

        await Sut().Handle(new SetUserFeatureFlagOverrideCommand(targetUserId, "Feature.X", false), CancellationToken.None);

        await _repository.Received(1).SetAsync(targetUserId, "Feature.X", false, "System", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(TestUserId.Value);
        var targetUserId = Guid.NewGuid();

        _repository.SetAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("connection failed"));

        var result = await Sut().Handle(new SetUserFeatureFlagOverrideCommand(targetUserId, "Feature.X", true), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Configuration.UserOverrideFailed", result.Error.ErrorCode);
    }
}
