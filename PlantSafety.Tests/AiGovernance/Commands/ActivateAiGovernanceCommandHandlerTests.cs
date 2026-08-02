// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AiGovernance.Commands
//  Layer:          Tests (Unit)
//  File:           ActivateAiGovernanceCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 AiGovernance full build-out.
//                  ActivateAiGovernanceCommandHandler must forward the caller's ETag to
//                  IAiGovernanceRepository.UpdateAsync, and must let a ConcurrencyConflictException
//                  raised by the repository propagate unchanged rather than swallowing it into a
//                  generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Commands.StartAuditingCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.AiGovernance.Commands;
using PlantSafety.Domain.AiGovernance.Repositories;
using PlantSafety.Domain.AiGovernance.ValueObjects;
using AiGovernanceAggregate = PlantSafety.Domain.AiGovernance.Aggregates.AiGovernance;

namespace PlantSafety.Tests.AiGovernance.Commands;

public sealed class ActivateAiGovernanceCommandHandlerTests : UnitTestBase
{
    private readonly IAiGovernanceRepository _repo = Substitute.For<IAiGovernanceRepository>();

    private ActivateAiGovernanceCommandHandler Sut() => new(_repo, NullLogger<ActivateAiGovernanceCommandHandler>());

    private static AiGovernanceAggregate CreatedAiGovernance() =>
        AiGovernanceAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            systemName: "Predictive Maintenance Model",
            aiUseCase: "Forecast equipment failure risk",
            riskClassification: "High",
            ownerUserId: TestUserId);

    private static ActivateAiGovernanceCommand ActivateCommandFor(AiGovernanceAggregate aggregate, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            AiGovernanceId: aggregate.AiGovernanceId.Value,
            PerformedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var aggregate = CreatedAiGovernance();
        _repo.GetByIdAsync(Arg.Any<AiGovernanceId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(aggregate);

        var command = ActivateCommandFor(aggregate, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(aggregate, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var aggregate = CreatedAiGovernance();
        _repo.GetByIdAsync(Arg.Any<AiGovernanceId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(aggregate);
        _repo.UpdateAsync(Arg.Any<AiGovernanceAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("AiGovernance was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ActivateCommandFor(aggregate, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenAiGovernanceDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<AiGovernanceId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AiGovernanceAggregate?)null);

        var command = ActivateCommandFor(CreatedAiGovernance(), null);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AiGovernance.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing AiGovernance record must map to HTTP 404, not the default 500");
    }
}
