// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.PredictiveSafety.Commands
//  Layer:          Tests (Unit)
//  File:           AcknowledgePredictiveSafetyAlertCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Intelligence.PredictiveSafety full build-out.
//                  AcknowledgePredictiveSafetyAlertCommandHandler must forward the caller's ETag to
//                  IPredictiveSafetyAlertRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.ContinuousImprovement.RCA.Commands
//                  .StartAnalysisRCACommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Intelligence.PredictiveSafety.Commands;
using PlantSafety.Domain.Intelligence.PredictiveSafety.Interfaces;
using PlantSafety.Domain.Intelligence.PredictiveSafety.ValueObjects.Ids;
using PredictiveSafetyAlertAggregate = PlantSafety.Domain.Intelligence.PredictiveSafety.Aggregates.PredictiveSafetyAlert;

namespace PlantSafety.Tests.Intelligence.PredictiveSafety.Commands;

public sealed class AcknowledgePredictiveSafetyAlertCommandHandlerTests : UnitTestBase
{
    private readonly IPredictiveSafetyAlertRepository _repo = Substitute.For<IPredictiveSafetyAlertRepository>();

    private AcknowledgePredictiveSafetyAlertCommandHandler Sut() => new(_repo, NullLogger<AcknowledgePredictiveSafetyAlertCommandHandler>());

    private static PredictiveSafetyAlertAggregate ActiveAlert() =>
        PredictiveSafetyAlertAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            riskCategory: "Fatigue",
            predictedRiskScore: 0.87,
            modelVersion: "2.1.0",
            generatedBy: TestUserId);

    private static AcknowledgePredictiveSafetyAlertCommand AcknowledgeCommandFor(PredictiveSafetyAlertAggregate alert, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            PredictiveSafetyAlertId: alert.PredictiveSafetyAlertId.Value,
            AcknowledgedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var alert = ActiveAlert();
        _repo.GetByIdAsync(Arg.Any<PredictiveSafetyAlertId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(alert);

        var command = AcknowledgeCommandFor(alert, "0x0000000000001234");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Acknowledged");
        await _repo.Received(1).UpdateAsync(alert, "0x0000000000001234", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var alert = ActiveAlert();
        _repo.GetByIdAsync(Arg.Any<PredictiveSafetyAlertId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(alert);
        _repo.UpdateAsync(Arg.Any<PredictiveSafetyAlertAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("PredictiveSafetyAlert was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = AcknowledgeCommandFor(alert, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenPredictiveSafetyAlertDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<PredictiveSafetyAlertId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PredictiveSafetyAlertAggregate?)null);

        var command = AcknowledgeCommandFor(ActiveAlert(), null);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Intelligence.PredictiveSafety.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing PredictiveSafetyAlert record must map to HTTP 404, not the default 500");
    }
}
