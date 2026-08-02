// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.ContractorVisitor.Commands
//  Layer:          Tests (Unit)
//  File:           ActivateContractorVisitorCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 ContractorVisitor full build-out.
//                  ActivateContractorVisitorCommandHandler must forward the caller's ETag to
//                  IContractorVisitorRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Commands.StartAuditingCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.ContractorVisitor.Commands;
using PlantSafety.Domain.ContractorVisitor.Repositories;
using PlantSafety.Domain.ContractorVisitor.ValueObjects;
using ContractorVisitorAggregate = PlantSafety.Domain.ContractorVisitor.Aggregates.ContractorVisitor;

namespace PlantSafety.Tests.ContractorVisitor.Commands;

public sealed class ActivateContractorVisitorCommandHandlerTests : UnitTestBase
{
    private readonly IContractorVisitorRepository _repo = Substitute.For<IContractorVisitorRepository>();

    private ActivateContractorVisitorCommandHandler Sut() => new(_repo, NullLogger<ActivateContractorVisitorCommandHandler>());

    private static ContractorVisitorAggregate CreatedContractorVisitor() =>
        ContractorVisitorAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            fullName: "Jordan Smith",
            companyName: "Acme Contracting LLC",
            visitorType: "Contractor",
            purposeOfVisit: "Scheduled HVAC maintenance",
            hostUserId: TestUserId);

    private static ActivateContractorVisitorCommand ActivateCommandFor(ContractorVisitorAggregate aggregate, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            ContractorVisitorId: aggregate.ContractorVisitorId.Value,
            PerformedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var aggregate = CreatedContractorVisitor();
        _repo.GetByIdAsync(Arg.Any<ContractorVisitorId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(aggregate);

        var command = ActivateCommandFor(aggregate, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(aggregate, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var aggregate = CreatedContractorVisitor();
        _repo.GetByIdAsync(Arg.Any<ContractorVisitorId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(aggregate);
        _repo.UpdateAsync(Arg.Any<ContractorVisitorAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("ContractorVisitor was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ActivateCommandFor(aggregate, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenContractorVisitorDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<ContractorVisitorId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ContractorVisitorAggregate?)null);

        var command = ActivateCommandFor(CreatedContractorVisitor(), null);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("ContractorVisitor.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing ContractorVisitor record must map to HTTP 404, not the default 500");
    }
}
