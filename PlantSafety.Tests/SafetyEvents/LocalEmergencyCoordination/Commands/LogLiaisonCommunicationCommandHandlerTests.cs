// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Commands
//  Layer:          Tests (Unit)
//  File:           LogLiaisonCommunicationCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for LogLiaisonCommunicationCommandHandler. Isolated,
//                  deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Interfaces;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;
using LocalEmergencyCoordinationAggregate = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Commands;

public sealed class LogLiaisonCommunicationCommandHandlerTests : UnitTestBase
{
    private readonly ILocalEmergencyCoordinationRepository _repo = Substitute.For<ILocalEmergencyCoordinationRepository>();

    private LogLiaisonCommunicationCommandHandler Sut() => new(_repo, NullLogger<LogLiaisonCommunicationCommandHandler>());

    private static LogLiaisonCommunicationCommand CommandFor(LocalEmergencyCoordinationAggregate agg, Guid liaisonId, string? eTag = null) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        LocalEmergencyCoordinationId: agg.LocalEmergencyCoordinationId.Value,
        ExternalResponderLiaisonId: liaisonId,
        Message: "Requesting additional resources.",
        LoggedByUserId: TestUserId.Value)
    { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_LogsCommunicationAndForwardsCallerEtag()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        var liaison = agg.AssignExternalResponderLiaison(null, "County EMS", TestUserId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(agg);

        var result = await Sut().Handle(CommandFor(agg, liaison.ExternalResponderLiaisonId.Value, "0x00000000000BBBBB"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.LiaisonLogEntryId.Should().NotBe(Guid.Empty);
        await _repo.Received(1).UpdateAsync(agg, "0x00000000000BBBBB", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCaseDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((LocalEmergencyCoordinationAggregate?)null);

        var result = await Sut().Handle(CommandFor(LocalEmergencyCoordinationTestSupport.NewCase(), Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenLiaisonDoesNotExistOnAggregate_ReturnsGenericFailure()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        _repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(agg);

        var result = await Sut().Handle(CommandFor(agg, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.LocalEmergencyCoordination.LogLiaisonCommunicationFailed");
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        var liaison = agg.AssignExternalResponderLiaison(null, "County EMS", TestUserId, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(agg);
        _repo.UpdateAsync(Arg.Any<LocalEmergencyCoordinationAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("stale ETag", TestCorrelationId.Value));

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(CommandFor(agg, liaison.ExternalResponderLiaisonId.Value, "stale"), CancellationToken.None));
    }
}
