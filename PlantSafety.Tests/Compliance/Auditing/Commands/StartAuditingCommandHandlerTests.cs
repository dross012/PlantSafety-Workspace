// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Auditing.Commands
//  Layer:          Tests (Unit)
//  File:           StartAuditingCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Compliance.Auditing full build-out.
//                  StartAuditingCommandHandler must forward the caller's ETag to
//                  IAuditingRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.Auditing.Commands;
using PlantSafety.Domain.Compliance.Auditing.Interfaces;
using PlantSafety.Domain.Compliance.Auditing.ValueObjects.Ids;
using AuditingAggregate = PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing;

namespace PlantSafety.Tests.Compliance.Auditing.Commands;

public sealed class StartAuditingCommandHandlerTests : UnitTestBase
{
    private readonly IAuditingRepository _repo = Substitute.For<IAuditingRepository>();

    private StartAuditingCommandHandler Sut() => new(_repo, NullLogger<StartAuditingCommandHandler>());

    private static AuditingAggregate CreatedAuditing() =>
        AuditingAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            auditTitle: "Q3 Internal Safety Audit",
            auditType: "Internal",
            scope: "Line 3 confined space program",
            auditorId: TestUserId);

    private static StartAuditingCommand StartCommandFor(AuditingAggregate auditing, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            AuditingId: auditing.AuditingId.Value,
            StartedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var auditing = CreatedAuditing();
        _repo.GetByIdAsync(Arg.Any<AuditingId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(auditing);

        var command = StartCommandFor(auditing, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(auditing, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var auditing = CreatedAuditing();
        _repo.GetByIdAsync(Arg.Any<AuditingId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(auditing);
        _repo.UpdateAsync(Arg.Any<AuditingAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("Auditing was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = StartCommandFor(auditing, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
