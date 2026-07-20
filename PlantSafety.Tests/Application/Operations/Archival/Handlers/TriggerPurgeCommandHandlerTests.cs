// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Archival.Handlers
//  Layer:          Tests (Unit)
//  File:           TriggerPurgeCommandHandlerTests.cs
//  Created:        2026-07-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for TriggerPurgeCommandHandler.
// ============================================================================
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using PlantSafety.Application.Operations.Archival.Commands;
using PlantSafety.Application.Operations.Archival.Handlers;
using PlantSafety.Application.Operations.Archival.Models;
using PlantSafety.Application.Operations.Archival.Repositories;
using PlantSafety.Tests.Common.TestBase;

using Xunit;

namespace PlantSafety.Tests.Application.Operations.Archival.Handlers;

public sealed class TriggerPurgeCommandHandlerTests : UnitTestBase
{
    private readonly IArchivalRepository _repository = Substitute.For<IArchivalRepository>();

    private TriggerPurgeCommandHandler Sut() => new(_repository, NullLogger<TriggerPurgeCommandHandler>());

    [Fact]
    public async Task Handle_WithSingleTarget_ShouldReturnSuccessResult()
    {
        IReadOnlyList<PurgeTriggerResultDto> expected =
        [
            new PurgeTriggerResultDto(PurgeTarget.AuditLog, "AuditDB.dbo.AuditLog", "Completed", Guid.NewGuid(), FixedUtcNow, FixedUtcNow, "SUCCESS: 10 row(s) purged.")
        ];
        _repository.TriggerPurgeAsync(PurgeTarget.AuditLog, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new TriggerPurgeCommand(PurgeTarget.AuditLog), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public async Task Handle_WithNullTarget_ShouldInvokeRepositoryWithAllTargets()
    {
        _repository.TriggerPurgeAsync(Arg.Any<PurgeTarget?>(), Arg.Any<CancellationToken>())
            .Returns(new List<PurgeTriggerResultDto>());

        await Sut().Handle(new TriggerPurgeCommand(null), CancellationToken.None);

        await _repository.Received(1).TriggerPurgeAsync(null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.TriggerPurgeAsync(Arg.Any<PurgeTarget?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("connection failed"));

        var result = await Sut().Handle(new TriggerPurgeCommand(PurgeTarget.ErrorLog), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Archival.TriggerFailed", result.Error.ErrorCode);
    }
}
