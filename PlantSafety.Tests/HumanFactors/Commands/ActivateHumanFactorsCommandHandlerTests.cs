// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.Commands
//  Layer:          Tests (Unit)
//  File:           ActivateHumanFactorsCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 HumanFactors full build-out.
//                  ActivateHumanFactorsCommandHandler must forward the caller's ETag to
//                  IHumanFactorsRepository.UpdateAsync, must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError, must map a missing aggregate to
//                  ErrorSeverity.NotFound, and must not cross tenant boundaries. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Commands.StartAuditingCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.HumanFactors.Commands;
using PlantSafety.Domain.HumanFactors.Repositories;
using PlantSafety.Domain.HumanFactors.ValueObjects;
using HumanFactorsAggregate = PlantSafety.Domain.HumanFactors.Aggregates.HumanFactors;

namespace PlantSafety.Tests.HumanFactors.Commands;

public sealed class ActivateHumanFactorsCommandHandlerTests : UnitTestBase
{
    private readonly IHumanFactorsRepository _repo = Substitute.For<IHumanFactorsRepository>();

    private ActivateHumanFactorsCommandHandler Sut() => new(_repo, NullLogger<ActivateHumanFactorsCommandHandler>());

    private static HumanFactorsAggregate CreatedHumanFactors() =>
        HumanFactorsAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            factorType: "Fatigue",
            severityRating: 3,
            description: "Extended overtime shift pattern flagged for review.");

    private static ActivateHumanFactorsCommand ActivateCommandFor(HumanFactorsAggregate humanFactors, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            HumanFactorsId: humanFactors.HumanFactorsId.Value,
            PerformedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var humanFactors = CreatedHumanFactors();
        _repo.GetByIdAsync(Arg.Any<HumanFactorsId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(humanFactors);

        var command = ActivateCommandFor(humanFactors, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(humanFactors, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var humanFactors = CreatedHumanFactors();
        _repo.GetByIdAsync(Arg.Any<HumanFactorsId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(humanFactors);
        _repo.UpdateAsync(Arg.Any<HumanFactorsAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("HumanFactors was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ActivateCommandFor(humanFactors, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenHumanFactorsDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<HumanFactorsId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((HumanFactorsAggregate?)null);

        var command = new ActivateHumanFactorsCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            HumanFactorsId: Guid.NewGuid(),
            PerformedByUserId: TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing HumanFactors record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenHumanFactorsBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_HumanFactorsAggregate_GetById filters by TenantId in
        // SQL, so a lookup scoped to TestTenantId for a record that actually belongs to another
        // tenant returns no row at all -- the repository is not stubbed for that
        // (HumanFactorsId, TestTenantId) combination and NSubstitute's default (null) applies,
        // exactly as the real proc would.
        var humanFactorsId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(Arg.Is<HumanFactorsId>(h => h.Value == humanFactorsId), otherTenantId, Arg.Any<CancellationToken>())
            .Returns(CreatedHumanFactors());

        var command = new ActivateHumanFactorsCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            HumanFactorsId: humanFactorsId,
            PerformedByUserId: TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.NotFound");
    }
}
