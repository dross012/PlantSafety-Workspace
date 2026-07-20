// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Backup.Handlers
//  Layer:          Tests (Unit)
//  File:           GetBackupHistoryQueryHandlerTests.cs
//  Created:        2026-07-19
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetBackupHistoryQueryHandler.
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

public sealed class GetBackupHistoryQueryHandlerTests : UnitTestBase
{
    private readonly IBackupRepository _repository = Substitute.For<IBackupRepository>();

    private GetBackupHistoryQueryHandler Sut() => new(_repository, NullLogger<GetBackupHistoryQueryHandler>());

    [Fact]
    public async Task Handle_ShouldReturnItemsFromRepository()
    {
        IReadOnlyList<BackupHistoryItemDto> expected =
        [
            new BackupHistoryItemDto("PlantSafety.CoreDB", "Full", FixedUtcNow, 128.5m, @"C:\Backups\core.bak")
        ];
        _repository.GetBackupHistoryAsync(null, 50, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new GetBackupHistoryQuery(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }

    [Fact]
    public async Task Handle_ShouldPassDatabaseNameAndTopThrough()
    {
        _repository.GetBackupHistoryAsync("PlantSafety.CoreDB", 10, Arg.Any<CancellationToken>())
            .Returns(new List<BackupHistoryItemDto>());

        await Sut().Handle(new GetBackupHistoryQuery("PlantSafety.CoreDB", 10), CancellationToken.None);

        await _repository.Received(1).GetBackupHistoryAsync("PlantSafety.CoreDB", 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.GetBackupHistoryAsync(Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("msdb unreachable"));

        var result = await Sut().Handle(new GetBackupHistoryQuery(null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Backup.HistoryRetrievalFailed", result.Error.ErrorCode);
    }
}
