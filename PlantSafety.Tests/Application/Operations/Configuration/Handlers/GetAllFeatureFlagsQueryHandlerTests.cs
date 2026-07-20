// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Configuration.Handlers
//  Layer:          Tests (Unit)
//  File:           GetAllFeatureFlagsQueryHandlerTests.cs
//  Created:        2026-07-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetAllFeatureFlagsQueryHandler.
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

public sealed class GetAllFeatureFlagsQueryHandlerTests : UnitTestBase
{
    private readonly IFeatureFlagRepository _repository = Substitute.For<IFeatureFlagRepository>();

    private GetAllFeatureFlagsQueryHandler Sut() => new(_repository, NullLogger<GetAllFeatureFlagsQueryHandler>());

    [Fact]
    public async Task Handle_ShouldReturnFlagsFromEveryDatabase()
    {
        IReadOnlyList<FeatureFlagDto> expected =
        [
            new FeatureFlagDto("ArchiveDB", "RetentionPurge.Enabled", true, "Ops kill-switch.", FixedUtcNow, "System"),
            new FeatureFlagDto("IntegrationDB", "ServiceBroker.ReplayProcessingEnabled", true, "Ops kill-switch.", FixedUtcNow, "System")
        ];
        _repository.GetAllAcrossDatabasesAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new GetAllFeatureFlagsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.GetAllAcrossDatabasesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("connection failed"));

        var result = await Sut().Handle(new GetAllFeatureFlagsQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Configuration.RetrievalFailed", result.Error.ErrorCode);
    }
}
