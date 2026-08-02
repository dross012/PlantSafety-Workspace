// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.CrisisManagement.Commands
//  Layer:          Tests (Unit)
//  File:           RespondToCrisisManagementCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 SafetyEvents.CrisisManagement full build-out.
//                  RespondToCrisisManagementCommandHandler must forward the caller's ETag to
//                  ICrisisManagementRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.SafetyEvents.CrisisManagement.Commands;
using PlantSafety.Domain.SafetyEvents.CrisisManagement.Interfaces;
using PlantSafety.Domain.SafetyEvents.CrisisManagement.ValueObjects.Ids;
using CrisisManagementAggregate = PlantSafety.Domain.SafetyEvents.CrisisManagement.Aggregates.CrisisManagement;

namespace PlantSafety.Tests.SafetyEvents.CrisisManagement.Commands;

public sealed class RespondToCrisisManagementCommandHandlerTests : UnitTestBase
{
    private readonly ICrisisManagementRepository _repo = Substitute.For<ICrisisManagementRepository>();

    private RespondToCrisisManagementCommandHandler Sut() => new(_repo, NullLogger<RespondToCrisisManagementCommandHandler>());

    private static CrisisManagementAggregate DeclaredCrisis() =>
        CrisisManagementAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            title: "Tank Farm Fire",
            severity: "Critical",
            declaredAtUtc: FixedTimestamp);

    private static RespondToCrisisManagementCommand RespondToCommandFor(CrisisManagementAggregate crisis, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            CrisisManagementId: crisis.CrisisManagementId.Value,
            RespondedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var crisis = DeclaredCrisis();
        _repo.GetByIdAsync(Arg.Any<CrisisManagementId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(crisis);

        var command = RespondToCommandFor(crisis, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(crisis, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var crisis = DeclaredCrisis();
        _repo.GetByIdAsync(Arg.Any<CrisisManagementId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(crisis);
        _repo.UpdateAsync(Arg.Any<CrisisManagementAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("CrisisManagement was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = RespondToCommandFor(crisis, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
