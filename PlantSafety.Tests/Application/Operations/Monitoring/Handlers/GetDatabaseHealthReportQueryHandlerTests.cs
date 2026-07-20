// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Monitoring.Handlers
//  Layer:          Tests (Unit)
//  File:           GetDatabaseHealthReportQueryHandlerTests.cs
//  Created:        2026-07-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetDatabaseHealthReportQueryHandler.
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

public sealed class GetDatabaseHealthReportQueryHandlerTests : UnitTestBase
{
    private readonly IDatabaseHealthReportProvider _provider = Substitute.For<IDatabaseHealthReportProvider>();

    private GetDatabaseHealthReportQueryHandler Sut() => new(_provider, NullLogger<GetDatabaseHealthReportQueryHandler>());

    [Fact]
    public async Task Handle_ShouldReturnReportFromProvider()
    {
        var expected = new DatabaseHealthReportDto(
            "Healthy",
            FixedUtcNow,
            42.0,
            [new HealthCheckEntryDto("database", "Healthy", "Database connection is healthy.", 12.0, ["ready"])]);

        _provider.GetHealthReportAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new GetDatabaseHealthReportQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Healthy", result.Value.OverallStatus);
        Assert.Single(result.Value.Entries);
    }

    [Fact]
    public async Task Handle_WhenProviderThrows_ShouldReturnFailureResult()
    {
        _provider.GetHealthReportAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("HealthCheckService unavailable"));

        var result = await Sut().Handle(new GetDatabaseHealthReportQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Monitoring.HealthReportRetrievalFailed", result.Error.ErrorCode);
    }
}
