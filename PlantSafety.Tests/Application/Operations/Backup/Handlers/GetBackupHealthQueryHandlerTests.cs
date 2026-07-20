// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Backup.Handlers
//  Layer:          Tests (Unit)
//  File:           GetBackupHealthQueryHandlerTests.cs
//  Created:        2026-07-19
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetBackupHealthQueryHandler.
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

public sealed class GetBackupHealthQueryHandlerTests : UnitTestBase
{
    private readonly IBackupRepository _repository = Substitute.For<IBackupRepository>();

    private GetBackupHealthQueryHandler Sut() => new(_repository, NullLogger<GetBackupHealthQueryHandler>());

    [Fact]
    public async Task Handle_ShouldReturnHealthFromRepository()
    {
        IReadOnlyList<BackupHealthDto> expected =
        [
            new BackupHealthDto("PlantSafety.CoreDB", FixedUtcNow, "Green", "Last FULL backup 2.0h ago.")
        ];
        _repository.GetBackupHealthAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new GetBackupHealthQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("Green", result.Value[0].HealthStatus);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.GetBackupHealthAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("sys.databases unreachable"));

        var result = await Sut().Handle(new GetBackupHealthQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Backup.HealthRetrievalFailed", result.Error.ErrorCode);
    }
}
