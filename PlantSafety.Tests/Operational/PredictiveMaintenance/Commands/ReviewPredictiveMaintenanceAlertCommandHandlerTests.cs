// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Operational.PredictiveMaintenance.Commands
//  Layer:          Tests (Unit)
//  File:           ReviewPredictiveMaintenanceAlertCommandHandlerTests.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Operational.PredictiveMaintenance full
//                  build-out. ReviewPredictiveMaintenanceAlertCommandHandler must forward the
//                  caller's ETag to IPredictiveMaintenanceAlertRepository.UpdateAsync, and must let
//                  a ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Operational.PredictiveMaintenance.Commands;
using PlantSafety.Domain.Operational.PredictiveMaintenance.Interfaces;
using PlantSafety.Domain.Operational.PredictiveMaintenance.ValueObjects.Ids;
using PredictiveMaintenanceAlertAggregate = PlantSafety.Domain.Operational.PredictiveMaintenance.Aggregates.PredictiveMaintenanceAlert;

namespace PlantSafety.Tests.Operational.PredictiveMaintenance.Commands;

public sealed class ReviewPredictiveMaintenanceAlertCommandHandlerTests : UnitTestBase
{
    private readonly IPredictiveMaintenanceAlertRepository _repo = Substitute.For<IPredictiveMaintenanceAlertRepository>();

    private ReviewPredictiveMaintenanceAlertCommandHandler Sut() => new(_repo, NullLogger<ReviewPredictiveMaintenanceAlertCommandHandler>());

    private static PredictiveMaintenanceAlertAggregate RaisedAlert() =>
        PredictiveMaintenanceAlertAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            equipmentId: Guid.NewGuid(),
            predictedFailureType: "BearingFailure",
            predictedAtUtc: FixedTimestamp);

    private static ReviewPredictiveMaintenanceAlertCommand ReviewCommandFor(PredictiveMaintenanceAlertAggregate alert, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            PredictiveMaintenanceAlertId: alert.PredictiveMaintenanceAlertId.Value,
            ReviewedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var alert = RaisedAlert();
        _repo.GetByIdAsync(Arg.Any<PredictiveMaintenanceAlertId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(alert);

        var command = ReviewCommandFor(alert, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(alert, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var alert = RaisedAlert();
        _repo.GetByIdAsync(Arg.Any<PredictiveMaintenanceAlertId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(alert);
        _repo.UpdateAsync(Arg.Any<PredictiveMaintenanceAlertAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("PredictiveMaintenanceAlert was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ReviewCommandFor(alert, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
