// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Configuration.Handlers
//  Layer:          Tests (Unit)
//  File:           SetTenantReleaseChannelCommandHandlerTests.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for SetTenantReleaseChannelCommandHandler.
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

public sealed class SetTenantReleaseChannelCommandHandlerTests : UnitTestBase
{
    private readonly ITenantReleaseChannelRepository _repository = Substitute.For<ITenantReleaseChannelRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();

    private SetTenantReleaseChannelCommandHandler Sut() =>
        new(_repository, _currentUserService, NullLogger<SetTenantReleaseChannelCommandHandler>());

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldPassCurrentUserIdAsUpdatedBy()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(TestUserId.Value);
        var tenantId = Guid.NewGuid();

        var expected = new TenantReleaseChannelDto(tenantId, TenantReleaseChannel.Beta, FixedUtcNow, TestUserId.Value.ToString());
        _repository.SetAsync(tenantId, TenantReleaseChannel.Beta, TestUserId.Value.ToString(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await Sut().Handle(new SetTenantReleaseChannelCommand(tenantId, TenantReleaseChannel.Beta), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _currentUserService.IsAuthenticated.Returns(true);
        _currentUserService.UserId.Returns(TestUserId.Value);
        var tenantId = Guid.NewGuid();

        _repository.SetAsync(Arg.Any<Guid>(), Arg.Any<TenantReleaseChannel>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("connection failed"));

        var result = await Sut().Handle(new SetTenantReleaseChannelCommand(tenantId, TenantReleaseChannel.Canary), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Configuration.ReleaseChannelFailed", result.Error.ErrorCode);
    }
}
