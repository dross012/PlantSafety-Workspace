// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Configuration.Handlers
//  Layer:          Tests (Unit)
//  File:           GetFeatureFlagsQueryHandlerTests.cs
//  Created:        2026-07-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetFeatureFlagsQueryHandler.
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
using PlantSafety.Tests.Common.TestBase;

using Xunit;

namespace PlantSafety.Tests.Application.Operations.Configuration.Handlers;

public sealed class GetFeatureFlagsQueryHandlerTests : UnitTestBase
{
    private readonly IFeatureFlagRepository _repository = Substitute.For<IFeatureFlagRepository>();

    private GetFeatureFlagsQueryHandler Sut() => new(_repository, NullLogger<GetFeatureFlagsQueryHandler>());

    [Fact]
    public async Task Handle_WithKnownDatabaseKey_ShouldReturnSuccessResult()
    {
        IReadOnlyList<FeatureFlagDto> expected =
        [
            new FeatureFlagDto("ArchiveDB", "RetentionPurge.Enabled", true, "Ops kill-switch.", FixedUtcNow, "System")
        ];
        _repository.GetAllAsync("ArchiveDB", Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new GetFeatureFlagsQuery("ArchiveDB"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task Handle_ShouldPassDatabaseKeyToRepository()
    {
        _repository.GetAllAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new List<FeatureFlagDto>());

        await Sut().Handle(new GetFeatureFlagsQuery("IntegrationDB"), CancellationToken.None);

        await _repository.Received(1).GetAllAsync("IntegrationDB", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.GetAllAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("connection failed"));

        var result = await Sut().Handle(new GetFeatureFlagsQuery("UnknownDB"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Configuration.RetrievalFailed", result.Error.ErrorCode);
    }
}
