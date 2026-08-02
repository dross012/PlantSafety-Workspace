// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Financials.Commands
//  Layer:          Tests (Unit)
//  File:           SubmitFinancialRecordCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Financials full build-out (new FinancialRecord
//                  aggregate root). SubmitFinancialRecordCommandHandler must forward the caller's
//                  ETag to IFinancialRecordRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Commands.StartAuditingCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Financials.Commands;
using PlantSafety.Domain.Financials.Aggregates;
using PlantSafety.Domain.Financials.Interfaces;
using PlantSafety.Domain.Financials.ValueObjects;

namespace PlantSafety.Tests.Financials.Commands;

public sealed class SubmitFinancialRecordCommandHandlerTests : UnitTestBase
{
    private readonly IFinancialRecordRepository _repo = Substitute.For<IFinancialRecordRepository>();

    private SubmitFinancialRecordCommandHandler Sut() => new(_repo, NullLogger<SubmitFinancialRecordCommandHandler>());

    private static FinancialRecord CreatedRecord() =>
        FinancialRecord.Create(
            tenantId: TestTenantId.Value,
            siteId: null,
            category: "Budget",
            amount: 1000m,
            recordedBy: TestUserId,
            recordedAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor);

    private static SubmitFinancialRecordCommand SubmitCommandFor(FinancialRecord record, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            FinancialRecordId: record.FinancialsId.Value,
            ActorUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var record = CreatedRecord();
        _repo.GetByIdAsync(Arg.Any<FinancialsId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(record);

        var command = SubmitCommandFor(record, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(record, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var record = CreatedRecord();
        _repo.GetByIdAsync(Arg.Any<FinancialsId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(record);
        _repo.UpdateAsync(Arg.Any<FinancialRecord>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("FinancialRecord was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = SubmitCommandFor(record, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenFinancialRecordNotFound_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<FinancialsId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((FinancialRecord?)null);

        var command = new SubmitFinancialRecordCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            FinancialRecordId: Guid.NewGuid(),
            ActorUserId: TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Financials.FinancialRecord.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing FinancialRecord must map to HTTP 404, not the default 500");
    }
}
