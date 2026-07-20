// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Backup.Handlers
//  Layer:          Tests (Unit)
//  File:           TriggerBackupCommandHandlerTests.cs
//  Created:        2026-07-19
//  SchemaVersion:  2026.1
//  Description:    Unit tests for TriggerBackupCommandHandler.
// ============================================================================
using System;
using System.Threading;
using System.Threading.Tasks;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PlantSafety.Application.Operations.Backup.Commands;
using PlantSafety.Application.Operations.Backup.Handlers;
using PlantSafety.Application.Operations.Backup.Models;
using PlantSafety.Application.Operations.Backup.Repositories;
using PlantSafety.Tests.Common.TestBase;

using Xunit;

namespace PlantSafety.Tests.Application.Operations.Backup.Handlers;

public sealed class TriggerBackupCommandHandlerTests : UnitTestBase
{
    private readonly IBackupRepository _repository = Substitute.For<IBackupRepository>();

    private TriggerBackupCommandHandler Sut() => new(_repository, NullLogger<TriggerBackupCommandHandler>());

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessResult()
    {
        var expected = new BackupTriggerResultDto(BackupType.Full, 1234, FixedUtcNow, "Started", @"C:\Logs");
        _repository.TriggerBackupAsync(BackupType.Full, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new TriggerBackupCommand(BackupType.Full), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldInvokeRepositoryWithRequestedType()
    {
        _repository.TriggerBackupAsync(Arg.Any<BackupType>(), Arg.Any<CancellationToken>())
            .Returns(new BackupTriggerResultDto(BackupType.Diff, null, FixedUtcNow, "Started", @"C:\Logs"));

        await Sut().Handle(new TriggerBackupCommand(BackupType.Diff), CancellationToken.None);

        await _repository.Received(1).TriggerBackupAsync(BackupType.Diff, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.TriggerBackupAsync(Arg.Any<BackupType>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("process launch failed"));

        var result = await Sut().Handle(new TriggerBackupCommand(BackupType.Full), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Backup.TriggerFailed", result.Error.ErrorCode);
    }
}
