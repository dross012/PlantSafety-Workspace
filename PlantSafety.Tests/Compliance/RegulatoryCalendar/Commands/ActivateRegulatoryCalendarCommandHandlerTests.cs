// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.RegulatoryCalendar.Commands
//  Layer:          Tests (Unit)
//  File:           ActivateRegulatoryCalendarCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Compliance.RegulatoryCalendar full build-out.
//                  ActivateRegulatoryCalendarCommandHandler must forward the caller's ETag to
//                  IRegulatoryCalendarRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Permits.Commands.ApprovePermitsCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Commands;
using PlantSafety.Domain.Compliance.RegulatoryCalendar.Interfaces;
using PlantSafety.Domain.Compliance.RegulatoryCalendar.ValueObjects.Ids;
using RegulatoryCalendarAggregate = PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates.RegulatoryCalendar;

namespace PlantSafety.Tests.Compliance.RegulatoryCalendar.Commands;

public sealed class ActivateRegulatoryCalendarCommandHandlerTests : UnitTestBase
{
    private readonly IRegulatoryCalendarRepository _repo = Substitute.For<IRegulatoryCalendarRepository>();

    private ActivateRegulatoryCalendarCommandHandler Sut() => new(_repo, NullLogger<ActivateRegulatoryCalendarCommandHandler>());

    private static RegulatoryCalendarAggregate CreatedRegulatoryCalendar() =>
        RegulatoryCalendarAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            eventTitle: "Annual OSHA Filing",
            regulatoryBody: "OSHA",
            jurisdictionCode: "US-FED",
            complianceCategory: "Reporting",
            responsiblePartyId: TestUserId,
            dueAtUtc: FixedTimestamp,
            reminderLeadDays: 30);

    private static ActivateRegulatoryCalendarCommand ActivateCommandFor(RegulatoryCalendarAggregate regulatoryCalendar, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            RegulatoryCalendarId: regulatoryCalendar.RegulatoryCalendarId.Value,
            ActivatedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var regulatoryCalendar = CreatedRegulatoryCalendar();
        _repo.GetByIdAsync(Arg.Any<RegulatoryCalendarId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(regulatoryCalendar);

        var command = ActivateCommandFor(regulatoryCalendar, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(regulatoryCalendar, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var regulatoryCalendar = CreatedRegulatoryCalendar();
        _repo.GetByIdAsync(Arg.Any<RegulatoryCalendarId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(regulatoryCalendar);
        _repo.UpdateAsync(Arg.Any<RegulatoryCalendarAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("RegulatoryCalendar was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ActivateCommandFor(regulatoryCalendar, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }
}
