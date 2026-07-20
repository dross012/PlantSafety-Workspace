// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Backup.Handlers
//  Layer:          Tests (Unit)
//  File:           GetDatabaseSizeHistoryQueryHandlerTests.cs
//  Created:        2026-07-19
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetDatabaseSizeHistoryQueryHandler.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PlantSafety.Application.Operations.Backup.Handlers;
using PlantSafety.Application.Operations.Backup.Models;
using PlantSafety.Application.Operations.Backup.Queries;
using PlantSafety.Application.Operations.Backup.Repositories;
using PlantSafety.Tests.Common.TestBase;

using Xunit;

namespace PlantSafety.Tests.Application.Operations.Backup.Handlers;

public sealed class GetDatabaseSizeHistoryQueryHandlerTests : UnitTestBase
{
    private readonly IBackupRepository _repository = Substitute.For<IBackupRepository>();

    private GetDatabaseSizeHistoryQueryHandler Sut() => new(_repository, NullLogger<GetDatabaseSizeHistoryQueryHandler>());

    [Fact]
    public async Task Handle_ShouldReturnItemsFromRepository()
    {
        IReadOnlyList<DatabaseSizeSnapshotDto> expected =
        [
            new DatabaseSizeSnapshotDto(FixedUtcNow, "PlantSafety.CoreDB", "CoreDB_Data", "ROWS", 2048m, 1024m)
        ];
        _repository.GetDatabaseSizeHistoryAsync(null, 100, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new GetDatabaseSizeHistoryQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.GetDatabaseSizeHistoryAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("msdb unreachable"));

        var result = await Sut().Handle(new GetDatabaseSizeHistoryQuery(null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Backup.DatabaseSizeRetrievalFailed", result.Error.ErrorCode);
    }
}
