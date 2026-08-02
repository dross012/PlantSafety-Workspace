// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Procedures.Commands
//  Layer:          Tests (Unit)
//  File:           ApproveProceduresCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Compliance.Procedures full build-out.
//                  ApproveProceduresCommandHandler must forward the caller's ETag to
//                  IProceduresRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.Procedures.Commands;
using PlantSafety.Domain.Compliance.Procedures.Interfaces;
using PlantSafety.Domain.Compliance.Procedures.ValueObjects.Ids;
using ProceduresAggregate = PlantSafety.Domain.Compliance.Procedures.Aggregates.Procedures;

namespace PlantSafety.Tests.Compliance.Procedures.Commands;

public sealed class ApproveProceduresCommandHandlerTests : UnitTestBase
{
    private readonly IProceduresRepository _repo = Substitute.For<IProceduresRepository>();

    private ApproveProceduresCommandHandler Sut() => new(_repo, NullLogger<ApproveProceduresCommandHandler>());

    private static ProceduresAggregate CreatedProcedures() =>
        ProceduresAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            procedureTitle: "Confined Space Entry SOP",
            procedureNumber: "SOP-2026-0001",
            category: "ConfinedSpace",
            ownerId: TestUserId);

    private static ApproveProceduresCommand ApproveCommandFor(ProceduresAggregate procedures, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            ProceduresId: procedures.ProceduresId.Value,
            ApprovedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var procedures = CreatedProcedures();
        _repo.GetByIdAsync(Arg.Any<ProceduresId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(procedures);

        var command = ApproveCommandFor(procedures, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(procedures, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var procedures = CreatedProcedures();
        _repo.GetByIdAsync(Arg.Any<ProceduresId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(procedures);
        _repo.UpdateAsync(Arg.Any<ProceduresAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("Procedures was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ApproveCommandFor(procedures, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
