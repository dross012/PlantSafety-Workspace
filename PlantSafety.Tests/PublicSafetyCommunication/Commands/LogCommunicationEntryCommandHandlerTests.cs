// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Commands
//  Layer:          Tests (Unit)
//  File:           LogCommunicationEntryCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler tests for LogCommunicationEntryCommandHandler. Isolated, deterministic,
//                  <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Interfaces;
using LogEntryAggregate = PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry;

namespace PlantSafety.Tests.PublicSafetyCommunication.Commands;

public sealed class LogCommunicationEntryCommandHandlerTests : UnitTestBase
{
    private readonly ICommunicationLogEntryRepository _repo = Substitute.For<ICommunicationLogEntryRepository>();

    private LogCommunicationEntryCommandHandler Sut() => new(_repo, NullLogger<LogCommunicationEntryCommandHandler>());

    private static LogCommunicationEntryCommand ValidCommand(string direction = "Outbound", string medium = "Phone") => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        IncidentId: Guid.NewGuid(),
        RelatedIncidentNotificationId: null,
        RelatedPublicAlertBroadcastId: null,
        Direction: direction,
        Medium: medium,
        CounterpartyName: "County Fire Dispatch",
        Summary: "Notified county fire dispatch of the release.",
        OccurredAtUtc: FixedUtcNow)
    { ActorId = TestUserId.Value };

    [Fact]
    public async Task Handle_WithValidCommand_PersistsAggregateAndReturnsSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CommunicationLogEntryId.Should().NotBe(Guid.Empty);
        await _repo.Received(1).AddAsync(Arg.Any<LogEntryAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognisedDirection_ReturnsFailureWithoutPersisting()
    {
        var result = await Sut().Handle(ValidCommand(direction: "Sideways"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.CommunicationLogEntry.InvalidDirection");
        await _repo.DidNotReceive().AddAsync(Arg.Any<LogEntryAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognisedMedium_ReturnsFailureWithoutPersisting()
    {
        var result = await Sut().Handle(ValidCommand(medium: "CarrierPigeon"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.CommunicationLogEntry.InvalidMedium");
        await _repo.DidNotReceive().AddAsync(Arg.Any<LogEntryAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailure()
    {
        _repo.AddAsync(Arg.Any<LogEntryAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.CommunicationLogEntry.LogFailed");
    }
}
