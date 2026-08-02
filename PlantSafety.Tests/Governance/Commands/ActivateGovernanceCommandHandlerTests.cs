// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Governance.Commands
//  Layer:          Tests (Unit)
//  File:           ActivateGovernanceCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 Governance full build-out.
//                  ActivateGovernanceCommandHandler must forward the caller's ETag to
//                  IGovernanceRepository.UpdateAsync, must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError, must map a missing aggregate to
//                  ErrorSeverity.NotFound, and must not cross tenant boundaries. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Commands.StartAuditingCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Governance.Commands;
using PlantSafety.Domain.Governance.Repositories;
using PlantSafety.Domain.Governance.ValueObjects;
using GovernanceAggregate = PlantSafety.Domain.Governance.Aggregates.Governance;

namespace PlantSafety.Tests.Governance.Commands;

public sealed class ActivateGovernanceCommandHandlerTests : UnitTestBase
{
    private readonly IGovernanceRepository _repo = Substitute.For<IGovernanceRepository>();

    private ActivateGovernanceCommandHandler Sut() => new(_repo, NullLogger<ActivateGovernanceCommandHandler>());

    private static GovernanceAggregate CreatedGovernance() =>
        GovernanceAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            governanceArea: "PolicyOversight",
            ownerUserId: TestUserId);

    private static ActivateGovernanceCommand ActivateCommandFor(GovernanceAggregate governance, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            GovernanceId: governance.GovernanceId.Value,
            PerformedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var governance = CreatedGovernance();
        _repo.GetByIdAsync(Arg.Any<GovernanceId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(governance);

        var command = ActivateCommandFor(governance, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(governance, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var governance = CreatedGovernance();
        _repo.GetByIdAsync(Arg.Any<GovernanceId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(governance);
        _repo.UpdateAsync(Arg.Any<GovernanceAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("Governance was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ActivateCommandFor(governance, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenGovernanceDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<GovernanceId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((GovernanceAggregate?)null);

        var command = new ActivateGovernanceCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            GovernanceId: Guid.NewGuid(),
            PerformedByUserId: TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Governance.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Governance record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenGovernanceBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_GovernanceAggregate_GetById filters by TenantId in
        // SQL, so a lookup scoped to TestTenantId for a record that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (GovernanceId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var governanceId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(Arg.Is<GovernanceId>(g => g.Value == governanceId), otherTenantId, Arg.Any<CancellationToken>())
            .Returns(CreatedGovernance());

        var command = new ActivateGovernanceCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            GovernanceId: governanceId,
            PerformedByUserId: TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Governance.NotFound");
    }
}
