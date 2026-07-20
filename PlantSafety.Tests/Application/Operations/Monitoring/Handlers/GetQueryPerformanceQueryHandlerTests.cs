// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Monitoring.Handlers
//  Layer:          Tests (Unit)
//  File:           GetQueryPerformanceQueryHandlerTests.cs
//  Created:        2026-07-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetQueryPerformanceQueryHandler.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PlantSafety.Application.Operations.Monitoring.Handlers;
using PlantSafety.Application.Operations.Monitoring.Models;
using PlantSafety.Application.Operations.Monitoring.Queries;
using PlantSafety.Application.Operations.Monitoring.Repositories;
using PlantSafety.Tests.Common.TestBase;

using Xunit;

namespace PlantSafety.Tests.Application.Operations.Monitoring.Handlers;

public sealed class GetQueryPerformanceQueryHandlerTests : UnitTestBase
{
    private readonly IMonitoringRepository _repository = Substitute.For<IMonitoringRepository>();

    private GetQueryPerformanceQueryHandler Sut() => new(_repository, NullLogger<GetQueryPerformanceQueryHandler>());

    [Fact]
    public async Task Handle_ShouldReturnQueriesFromRepository()
    {
        IReadOnlyList<QueryPerformanceItemDto> expected =
        [
            new QueryPerformanceItemDto("CoreDB", 1, 1, "SELECT 1", 100, 5.0, 3.0, 10.0, FixedUtcNow)
        ];
        _repository.GetTopQueriesByCpuAsync("CoreDB", 20, 24, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new GetQueryPerformanceQuery("CoreDB"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("CoreDB", result.Value[0].DatabaseKey);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.GetTopQueriesByCpuAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Query Store unreachable"));

        var result = await Sut().Handle(new GetQueryPerformanceQuery("CoreDB"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Monitoring.QueryPerformanceRetrievalFailed", result.Error.ErrorCode);
    }
}
