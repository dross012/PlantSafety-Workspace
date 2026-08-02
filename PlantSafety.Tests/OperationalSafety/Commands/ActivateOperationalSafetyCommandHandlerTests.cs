// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.OperationalSafety.Commands
//  Layer:          Tests (Unit)
//  File:           ActivateOperationalSafetyCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 OperationalSafety full build-out.
//                  ActivateOperationalSafetyCommandHandler must forward the caller's ETag to
//                  IOperationalSafetyRepository.UpdateAsync, must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError, must map a
//                  missing aggregate to ErrorSeverity.NotFound, and must not cross tenant
//                  boundaries. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Commands.StartAuditingCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.OperationalSafety.Commands;
using PlantSafety.Domain.OperationalSafety.Repositories;
using PlantSafety.Domain.OperationalSafety.ValueObjects;
using OperationalSafetyAggregate = PlantSafety.Domain.OperationalSafety.Aggregates.OperationalSafety;

namespace PlantSafety.Tests.OperationalSafety.Commands;

public sealed class ActivateOperationalSafetyCommandHandlerTests : UnitTestBase
{
    private readonly IOperationalSafetyRepository _repo = Substitute.For<IOperationalSafetyRepository>();

    private ActivateOperationalSafetyCommandHandler Sut() => new(_repo, NullLogger<ActivateOperationalSafetyCommandHandler>());

    private static OperationalSafetyAggregate CreatedOperationalSafety() =>
        OperationalSafetyAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            safetyDomain: "LineOfFire",
            riskLevel: "Medium",
            ownerUserId: TestUserId);

    private static ActivateOperationalSafetyCommand ActivateCommandFor(OperationalSafetyAggregate operationalSafety, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            OperationalSafetyId: operationalSafety.OperationalSafetyId.Value,
            PerformedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var operationalSafety = CreatedOperationalSafety();
        _repo.GetByIdAsync(Arg.Any<OperationalSafetyId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(operationalSafety);

        var command = ActivateCommandFor(operationalSafety, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(operationalSafety, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var operationalSafety = CreatedOperationalSafety();
        _repo.GetByIdAsync(Arg.Any<OperationalSafetyId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(operationalSafety);
        _repo.UpdateAsync(Arg.Any<OperationalSafetyAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("OperationalSafety was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ActivateCommandFor(operationalSafety, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenOperationalSafetyDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<OperationalSafetyId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((OperationalSafetyAggregate?)null);

        var command = new ActivateOperationalSafetyCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            OperationalSafetyId: Guid.NewGuid(),
            PerformedByUserId: TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("OperationalSafety.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing OperationalSafety record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenOperationalSafetyBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_OperationalSafetyAggregate_GetById filters by
        // TenantId in SQL, so a lookup scoped to TestTenantId for a record that actually
        // belongs to another tenant returns no row at all -- the repository is not stubbed for
        // that (OperationalSafetyId, TestTenantId) combination and NSubstitute's default (null)
        // applies, exactly as the real proc would.
        var operationalSafetyId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(Arg.Is<OperationalSafetyId>(o => o.Value == operationalSafetyId), otherTenantId, Arg.Any<CancellationToken>())
            .Returns(CreatedOperationalSafety());

        var command = new ActivateOperationalSafetyCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            OperationalSafetyId: operationalSafetyId,
            PerformedByUserId: TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("OperationalSafety.NotFound");
    }
}
