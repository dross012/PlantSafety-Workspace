// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.LocalEmergencyCoordination.Commands
//  Layer:          Tests (Unit)
//  File:           ArchiveLocalEmergencyCoordinationCaseCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ArchiveLocalEmergencyCoordinationCaseCommandHandler. Isolated,
//                  deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Commands;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Interfaces;
using PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.ValueObjects.Ids;
using PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.TestSupport;
using LocalEmergencyCoordinationAggregate = PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Aggregates.LocalEmergencyCoordination;

namespace PlantSafety.Tests.SafetyEvents.LocalEmergencyCoordination.Commands;

public sealed class ArchiveLocalEmergencyCoordinationCaseCommandHandlerTests : UnitTestBase
{
    private readonly ILocalEmergencyCoordinationRepository _repo = Substitute.For<ILocalEmergencyCoordinationRepository>();

    private ArchiveLocalEmergencyCoordinationCaseCommandHandler Sut() => new(_repo, NullLogger<ArchiveLocalEmergencyCoordinationCaseCommandHandler>());

    private static ArchiveLocalEmergencyCoordinationCaseCommand CommandFor(LocalEmergencyCoordinationAggregate agg, string? eTag = null) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        LocalEmergencyCoordinationId: agg.LocalEmergencyCoordinationId.Value,
        ArchivedByUserId: TestUserId.Value)
    { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ArchivesClosedCaseAndForwardsCallerEtag()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(agg);

        var result = await Sut().Handle(CommandFor(agg, "0x00000000000EEEEE"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Archived");
        await _repo.Received(1).UpdateAsync(agg, "0x00000000000EEEEE", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCaseDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((LocalEmergencyCoordinationAggregate?)null);

        var result = await Sut().Handle(CommandFor(LocalEmergencyCoordinationTestSupport.NewCase()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenCaseNotClosed_ReturnsGenericFailure()
    {
        // Archive is illegal from anything but Closed -- the resulting
        // LocalEmergencyCoordinationInvalidStatusTransitionException falls into the handler's
        // generic catch(Exception).
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        _repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(agg);

        var result = await Sut().Handle(CommandFor(agg), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("SafetyEvents.LocalEmergencyCoordination.ArchiveFailed");
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var agg = LocalEmergencyCoordinationTestSupport.NewCase();
        agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<LocalEmergencyCoordinationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(agg);
        _repo.UpdateAsync(Arg.Any<LocalEmergencyCoordinationAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("stale ETag", TestCorrelationId.Value));

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(CommandFor(agg, "stale"), CancellationToken.None));
    }
}
