// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.Observation.Commands
//  Layer:          Tests (Unit)
//  File:           ActivateObservationCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 SafetyEvents.Observation full build-out.
//                  ActivateObservationCommandHandler must forward the caller's ETag to
//                  IObservationRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.Observation.Commands;
using PlantSafety.Domain.SafetyEvents.Observation.Interfaces;
using PlantSafety.Domain.SafetyEvents.Observation.ValueObjects.Ids;
using ObservationAggregate = PlantSafety.Domain.SafetyEvents.Observation.Aggregates.Observation;

namespace PlantSafety.Tests.SafetyEvents.Observation.Commands;

public sealed class ActivateObservationCommandHandlerTests : UnitTestBase
{
    private readonly IObservationRepository _repo = Substitute.For<IObservationRepository>();

    private ActivateObservationCommandHandler Sut() => new(_repo, NullLogger<ActivateObservationCommandHandler>());

    private static ObservationAggregate RecordedObservation() =>
        ObservationAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            observerId: TestUserId,
            observationType: "AtRiskBehavior",
            hazardCategory: "Slip/Trip",
            location: "Warehouse B",
            description: "Wet floor without signage",
            correctiveActionRequired: true);

    private static ActivateObservationCommand ActivateCommandFor(ObservationAggregate observation, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            ObservationId: observation.ObservationId.Value,
            ActivatedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var observation = RecordedObservation();
        _repo.GetByIdAsync(Arg.Any<ObservationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(observation);

        var command = ActivateCommandFor(observation, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(observation, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var observation = RecordedObservation();
        _repo.GetByIdAsync(Arg.Any<ObservationId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(observation);
        _repo.UpdateAsync(Arg.Any<ObservationAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("Observation was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ActivateCommandFor(observation, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
