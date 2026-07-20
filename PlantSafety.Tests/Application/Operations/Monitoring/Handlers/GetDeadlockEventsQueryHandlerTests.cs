// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.Operations.Monitoring.Handlers
//  Layer:          Tests (Unit)
//  File:           GetDeadlockEventsQueryHandlerTests.cs
//  Created:        2026-07-20
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetDeadlockEventsQueryHandler.
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

public sealed class GetDeadlockEventsQueryHandlerTests : UnitTestBase
{
    private readonly IMonitoringRepository _repository = Substitute.For<IMonitoringRepository>();

    private GetDeadlockEventsQueryHandler Sut() => new(_repository, NullLogger<GetDeadlockEventsQueryHandler>());

    [Fact]
    public async Task Handle_ShouldReturnDeadlocksFromRepository()
    {
        IReadOnlyList<DeadlockEventDto> expected =
        [
            new DeadlockEventDto(FixedUtcNow, "process1234", 2, "keylock (dbo.SomeTable)", "<deadlock/>")
        ];
        _repository.GetRecentDeadlocksAsync(20, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await Sut().Handle(new GetDeadlockEventsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(2, result.Value[0].ParticipantCount);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.GetRecentDeadlocksAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("system_health session unreachable"));

        var result = await Sut().Handle(new GetDeadlockEventsQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Operations.Monitoring.DeadlockRetrievalFailed", result.Error.ErrorCode);
    }
}
