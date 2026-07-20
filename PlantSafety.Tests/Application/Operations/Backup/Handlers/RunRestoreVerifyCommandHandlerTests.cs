// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Backup.Handlers
//  Layer:          Tests (Unit)
//  File:           RunRestoreVerifyCommandHandlerTests.cs
//  Created:        2026-07-19
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RunRestoreVerifyCommandHandler.
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

public sealed class RunRestoreVerifyCommandHandlerTests : UnitTestBase
{
    private readonly IBackupRepository _repository = Substitute.For<IBackupRepository>();

    private RunRestoreVerifyCommandHandler Sut() => new(_repository, NullLogger<RunRestoreVerifyCommandHandler>());

    [Fact]
    public async Task Handle_WithValidDatabaseName_ShouldReturnSuccessResult()
    {
        var expected = new RestoreVerifyResultDto(1, "PlantSafety.CoreDB", FixedUtcNow, FixedUtcNow, "OK", "CHECKDB passed.");
        _repository.RunRestoreVerifyAsync("PlantSafety.CoreDB", Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new RunRestoreVerifyCommand("PlantSafety.CoreDB"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Handle_WithMissingDatabaseName_ShouldReturnValidationFailure(string? databaseName)
    {
        var result = await Sut().Handle(new RunRestoreVerifyCommand(databaseName!), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Backup.RestoreVerify.InvalidDatabaseName", result.Error.ErrorCode);
        await _repository.DidNotReceive().RunRestoreVerifyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.RunRestoreVerifyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("restore-test proc failed"));

        var result = await Sut().Handle(new RunRestoreVerifyCommand("PlantSafety.CoreDB"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Backup.RestoreVerifyFailed", result.Error.ErrorCode);
    }
}
