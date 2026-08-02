// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.Commands
//  Layer:          Tests (Unit)
//  File:           ActivateEnvironmentalHealthCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 EnvironmentalHealth full build-out.
//                  ActivateEnvironmentalHealthCommandHandler must forward the caller's ETag to
//                  IEnvironmentalHealthRepository.UpdateAsync, must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError, must map a
//                  missing aggregate to ErrorSeverity.NotFound, and must not cross tenant
//                  boundaries. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Commands.StartAuditingCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.EnvironmentalHealth.Commands;
using PlantSafety.Domain.EnvironmentalHealth.Repositories;
using PlantSafety.Domain.EnvironmentalHealth.ValueObjects;
using EnvironmentalHealthAggregate = PlantSafety.Domain.EnvironmentalHealth.Aggregates.EnvironmentalHealth;

namespace PlantSafety.Tests.EnvironmentalHealth.Commands;

public sealed class ActivateEnvironmentalHealthCommandHandlerTests : UnitTestBase
{
    private readonly IEnvironmentalHealthRepository _repo = Substitute.For<IEnvironmentalHealthRepository>();

    private ActivateEnvironmentalHealthCommandHandler Sut() => new(_repo, NullLogger<ActivateEnvironmentalHealthCommandHandler>());

    private static EnvironmentalHealthAggregate CreatedEnvironmentalHealth() =>
        EnvironmentalHealthAggregate.Create(
            tenantId: TestTenantId.Value,
            siteId: null,
            monitoringType: "AirQuality",
            measuredValue: 42.5m,
            unit: "AQI",
            threshold: 100m,
            thresholdExceeded: false,
            evaluatedStatus: "Normal",
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor);

    private static ActivateEnvironmentalHealthCommand ActivateCommandFor(EnvironmentalHealthAggregate environmentalHealth, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            EnvironmentalHealthId: environmentalHealth.EnvironmentalHealthId.Value,
            PerformedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var environmentalHealth = CreatedEnvironmentalHealth();
        _repo.GetByIdAsync(Arg.Any<EnvironmentalHealthId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(environmentalHealth);

        var command = ActivateCommandFor(environmentalHealth, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(environmentalHealth, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var environmentalHealth = CreatedEnvironmentalHealth();
        _repo.GetByIdAsync(Arg.Any<EnvironmentalHealthId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(environmentalHealth);
        _repo.UpdateAsync(Arg.Any<EnvironmentalHealthAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("EnvironmentalHealth was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ActivateCommandFor(environmentalHealth, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenEnvironmentalHealthDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<EnvironmentalHealthId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((EnvironmentalHealthAggregate?)null);

        var command = new ActivateEnvironmentalHealthCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            EnvironmentalHealthId: Guid.NewGuid(),
            PerformedByUserId: TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("EnvironmentalHealth.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing EnvironmentalHealth record must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenEnvironmentalHealthBelongsToDifferentTenant_ReturnsNotFound()
    {
        // Mirrors production behavior: usp_EnvironmentalHealthAggregate_GetById filters by
        // TenantId in SQL, so a lookup scoped to TestTenantId for a record that actually
        // belongs to another tenant returns no row at all -- the repository is not stubbed for
        // that (EnvironmentalHealthId, TestTenantId) combination and NSubstitute's default
        // (null) applies, exactly as the real proc would.
        var environmentalHealthId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(Arg.Is<EnvironmentalHealthId>(e => e.Value == environmentalHealthId), otherTenantId, Arg.Any<CancellationToken>())
            .Returns(CreatedEnvironmentalHealth());

        var command = new ActivateEnvironmentalHealthCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            EnvironmentalHealthId: environmentalHealthId,
            PerformedByUserId: TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("EnvironmentalHealth.NotFound");
    }
}
