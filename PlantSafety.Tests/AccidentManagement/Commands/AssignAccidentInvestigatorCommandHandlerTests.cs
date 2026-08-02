// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AccidentManagement.Commands
//  Layer:          Tests (Unit)
//  File:           AssignAccidentInvestigatorCommandHandlerTests.cs
//  Created:        2026-07-29
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 AccidentManagement concurrency remediation.
//                  AssignAccidentInvestigatorCommandHandler must forward the caller's ETag to
//                  IAccidentRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. Mirrors
//                  PlantSafety.Tests.RiskManagement.Commands.ScoreRiskCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AccidentManagement.Commands;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.AccidentManagement.Interfaces;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;
using AccidentAggregate = PlantSafety.Domain.AccidentManagement.Aggregates.Accident;

namespace PlantSafety.Tests.AccidentManagement.Commands;

public sealed class AssignAccidentInvestigatorCommandHandlerTests : UnitTestBase
{
    private readonly IAccidentRepository _repo = Substitute.For<IAccidentRepository>();

    private AssignAccidentInvestigatorCommandHandler Sut() => new(_repo, NullLogger<AssignAccidentInvestigatorCommandHandler>());

    private static AccidentAggregate ReportedAccident() =>
        AccidentAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, accidentTitle: "Unguarded conveyor pinch point", location: "Line 3", severity: null, ownerId: TestUserId);

    private static AssignAccidentInvestigatorCommand AssignCommandFor(AccidentAggregate accident, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            RequestedAtUtc: FixedUtcNow,
            TenantId: TestTenantId.Value,
            AccidentId: accident.AccidentId.Value,
            InvestigatorId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var accident = ReportedAccident();
        _repo.GetByIdAsync(Arg.Any<AccidentId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(accident);

        var command = AssignCommandFor(accident, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(accident, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var accident = ReportedAccident();
        _repo.GetByIdAsync(Arg.Any<AccidentId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(accident);
        _repo.UpdateAsync(Arg.Any<AccidentAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("Accident was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = AssignCommandFor(accident, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
