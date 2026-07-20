// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Monitoring.Handlers
//  Layer:          Tests (Unit)
//  File:           GetErrorLogQueryHandlerTests.cs
//  Created:        2026-07-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetErrorLogQueryHandler.
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

public sealed class GetErrorLogQueryHandlerTests : UnitTestBase
{
    private readonly IMonitoringRepository _repository = Substitute.For<IMonitoringRepository>();

    private GetErrorLogQueryHandler Sut() => new(_repository, NullLogger<GetErrorLogQueryHandler>());

    [Fact]
    public async Task Handle_ShouldReturnErrorsFromRepository()
    {
        IReadOnlyList<ErrorLogItemDto> expected =
        [
            new ErrorLogItemDto(50002, 16, 1, "sp_Test_Create", 10, "Concurrency conflict.", FixedUtcNow, "Id=1", "detail", "PlantSafety.CoreDB", "sp_Test_Create")
        ];
        _repository.GetRecentErrorsAsync(null, null, 50, 0, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new GetErrorLogQuery(null, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(50002, result.Value[0].ErrorNumber);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.GetRecentErrorsAsync(
                Arg.Any<int?>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>(),
                Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>())
            .ThrowsAsync(new InvalidOperationException("ErrorDB unreachable"));

        var result = await Sut().Handle(new GetErrorLogQuery(null, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Monitoring.ErrorLogRetrievalFailed", result.Error.ErrorCode);
    }

    [Fact]
    public async Task Handle_ShouldPassSeverityBandAndSourceFiltersToRepository()
    {
        IReadOnlyList<ErrorLogItemDto> expected =
        [
            new ErrorLogItemDto(50003, 17, 1, "sp_Test_Update", 22, "Resource error.", FixedUtcNow, "Id=2", "detail", "PlantSafety.CoreDB", "sp_Test_Update")
        ];

        _repository.GetRecentErrorsAsync(
                11, FixedUtcNow, 50, 0, Arg.Any<CancellationToken>(),
                19, "PlantSafety.CoreDB", "sp_Test_Update", "sp_Test_Update")
            .Returns(expected);

        var query = new GetErrorLogQuery(
            MinSeverity: 11,
            SinceUtc: FixedUtcNow,
            MaxSeverity: 19,
            SourceDatabase: "PlantSafety.CoreDB",
            Module: "sp_Test_Update",
            ErrorProcedure: "sp_Test_Update");

        var result = await Sut().Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(50003, result.Value[0].ErrorNumber);

        await _repository.Received(1).GetRecentErrorsAsync(
            11, FixedUtcNow, 50, 0, Arg.Any<CancellationToken>(),
            19, "PlantSafety.CoreDB", "sp_Test_Update", "sp_Test_Update");
    }

    [Fact]
    public async Task Handle_WithNoNewFilters_ShouldPassNullsForNewParameters()
    {
        IReadOnlyList<ErrorLogItemDto> expected = [];
        _repository.GetRecentErrorsAsync(
                null, null, 50, 0, Arg.Any<CancellationToken>(),
                null, null, null, null)
            .Returns(expected);

        var result = await Sut().Handle(new GetErrorLogQuery(null, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);

        await _repository.Received(1).GetRecentErrorsAsync(
            null, null, 50, 0, Arg.Any<CancellationToken>(),
            null, null, null, null);
    }
}
