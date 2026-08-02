// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.ManagementOfChange.Commands
//  Layer:          Tests (Unit)
//  File:           ApproveManagementOfChangeCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Compliance.ManagementOfChange full build-out.
//                  ApproveManagementOfChangeCommandHandler must forward the caller's ETag to
//                  IManagementOfChangeRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged rather
//                  than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.ManagementOfChange.Commands;
using PlantSafety.Domain.Compliance.ManagementOfChange.Interfaces;
using PlantSafety.Domain.Compliance.ManagementOfChange.ValueObjects.Ids;
using ManagementOfChangeAggregate = PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates.ManagementOfChange;

namespace PlantSafety.Tests.Compliance.ManagementOfChange.Commands;

public sealed class ApproveManagementOfChangeCommandHandlerTests : UnitTestBase
{
    private readonly IManagementOfChangeRepository _repo = Substitute.For<IManagementOfChangeRepository>();

    private ApproveManagementOfChangeCommandHandler Sut() => new(_repo, NullLogger<ApproveManagementOfChangeCommandHandler>());

    private static ManagementOfChangeAggregate CreatedManagementOfChange() =>
        ManagementOfChangeAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            changeTitle: "Replace conveyor guard",
            changeType: "Equipment",
            justification: "Worn guard poses pinch hazard",
            riskLevel: "Medium",
            requestedByUserId: TestUserId);

    private static ApproveManagementOfChangeCommand ApproveCommandFor(ManagementOfChangeAggregate moc, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            ManagementOfChangeId: moc.ManagementOfChangeId.Value,
            ApprovedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var moc = CreatedManagementOfChange();
        _repo.GetByIdAsync(Arg.Any<ManagementOfChangeId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(moc);

        var command = ApproveCommandFor(moc, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(moc, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var moc = CreatedManagementOfChange();
        _repo.GetByIdAsync(Arg.Any<ManagementOfChangeId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(moc);
        _repo.UpdateAsync(Arg.Any<ManagementOfChangeAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("ManagementOfChange was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ApproveCommandFor(moc, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
