// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Monitoring.Handlers
//  Layer:          Tests (Unit)
//  File:           GetMonitoringDatabaseSizeQueryHandlerTests.cs
//  Created:        2026-07-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetMonitoringDatabaseSizeQueryHandler, confirming it
//                  delegates to the existing IBackupRepository rather than duplicating
//                  the msdb.dbo.PlantSafety_DatabaseSizeLog query.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PlantSafety.Application.Operations.Backup.Models;
using PlantSafety.Application.Operations.Backup.Repositories;
using PlantSafety.Application.Operations.Monitoring.Handlers;
using PlantSafety.Application.Operations.Monitoring.Queries;
using PlantSafety.Tests.Common.TestBase;

using Xunit;

namespace PlantSafety.Tests.Application.Operations.Monitoring.Handlers;

public sealed class GetMonitoringDatabaseSizeQueryHandlerTests : UnitTestBase
{
    private readonly IBackupRepository _backupRepository = Substitute.For<IBackupRepository>();

    private GetMonitoringDatabaseSizeQueryHandler Sut() => new(_backupRepository, NullLogger<GetMonitoringDatabaseSizeQueryHandler>());

    [Fact]
    public async Task Handle_ShouldDelegateToBackupRepository()
    {
        IReadOnlyList<DatabaseSizeSnapshotDto> expected =
        [
            new DatabaseSizeSnapshotDto(FixedUtcNow, "PlantSafety.CoreDB", "PlantSafety_CoreDB_Data", "ROWS", 512.0m, 400.0m)
        ];
        _backupRepository.GetDatabaseSizeHistoryAsync(null, 100, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new GetMonitoringDatabaseSizeQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        await _backupRepository.Received(1).GetDatabaseSizeHistoryAsync(null, 100, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _backupRepository.GetDatabaseSizeHistoryAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("msdb unreachable"));

        var result = await Sut().Handle(new GetMonitoringDatabaseSizeQuery(null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Monitoring.DatabaseSizeRetrievalFailed", result.Error.ErrorCode);
    }
}
