// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.RiskManagement.Commands
//  Layer:          Tests (Unit)
//  File:           ScoreRiskCommandHandlerTests.cs
//  Created:        2026-07-29
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 RiskManagement concurrency remediation.
//                  ScoreRiskCommandHandler must forward the caller's ETag to
//                  IRiskRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. Mirrors
//                  PlantSafety.Tests.PermitToWork.Commands.ApprovePermitWorkflowCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.RiskManagement.Commands;
using PlantSafety.Domain.RiskManagement.Interfaces;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;
using RiskAggregate = PlantSafety.Domain.RiskManagement.Aggregates.Risk;

namespace PlantSafety.Tests.RiskManagement.Commands;

public sealed class ScoreRiskCommandHandlerTests : UnitTestBase
{
    private readonly IRiskRepository _repo = Substitute.For<IRiskRepository>();

    private ScoreRiskCommandHandler Sut() => new(_repo, NullLogger<ScoreRiskCommandHandler>());

    private static RiskAggregate IdentifiedRisk() =>
        RiskAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            riskTitle: "Unguarded conveyor pinch point",
            location: "Line 3",
            hazardSource: "Mechanical",
            ownerId: TestUserId);

    private static ScoreRiskCommand ScoreCommandFor(RiskAggregate risk, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            RequestedAtUtc: FixedUtcNow,
            TenantId: TestTenantId.Value,
            RiskId: risk.RiskId.Value,
            Severity: 4,
            Likelihood: 3,
            ScoredBy: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var risk = IdentifiedRisk();
        _repo.GetByIdAsync(Arg.Any<RiskId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(risk);

        var command = ScoreCommandFor(risk, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(risk, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var risk = IdentifiedRisk();
        _repo.GetByIdAsync(Arg.Any<RiskId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(risk);
        _repo.UpdateAsync(Arg.Any<RiskAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("Risk was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ScoreCommandFor(risk, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
