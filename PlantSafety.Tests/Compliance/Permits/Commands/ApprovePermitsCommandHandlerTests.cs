// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Permits.Commands
//  Layer:          Tests (Unit)
//  File:           ApprovePermitsCommandHandlerTests.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Compliance.Permits full build-out.
//                  ApprovePermitsCommandHandler must forward the caller's ETag to
//                  IPermitsRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. Mirrors
//                  PlantSafety.Tests.RiskManagement.Commands.ScoreRiskCommandHandlerTests /
//                  PlantSafety.Tests.AccidentManagement.Commands.AssignAccidentInvestigatorCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.Permits.Commands;
using PlantSafety.Domain.Compliance.Permits.Interfaces;
using PlantSafety.Domain.Compliance.Permits.ValueObjects.Ids;
using PermitsAggregate = PlantSafety.Domain.Compliance.Permits.Aggregates.Permits;

namespace PlantSafety.Tests.Compliance.Permits.Commands;

public sealed class ApprovePermitsCommandHandlerTests : UnitTestBase
{
    private readonly IPermitsRepository _repo = Substitute.For<IPermitsRepository>();

    private ApprovePermitsCommandHandler Sut() => new(_repo, NullLogger<ApprovePermitsCommandHandler>());

    private static PermitsAggregate CreatedPermits() =>
        PermitsAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            permitType: "HotWork",
            permitNumber: "PTW-2026-0001",
            workDescription: "Welding on Line 3",
            location: "Line 3",
            hazardsIdentified: "Fire, fumes",
            requestedByUserId: TestUserId);

    private static ApprovePermitsCommand ApproveCommandFor(PermitsAggregate permits, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            PermitsId: permits.PermitsId.Value,
            ApprovedByUserId: TestUserId.Value,
            ValidityHours: 8)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var permits = CreatedPermits();
        _repo.GetByIdAsync(Arg.Any<PermitsId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(permits);

        var command = ApproveCommandFor(permits, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(permits, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var permits = CreatedPermits();
        _repo.GetByIdAsync(Arg.Any<PermitsId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(permits);
        _repo.UpdateAsync(Arg.Any<PermitsAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("Permits was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ApproveCommandFor(permits, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
