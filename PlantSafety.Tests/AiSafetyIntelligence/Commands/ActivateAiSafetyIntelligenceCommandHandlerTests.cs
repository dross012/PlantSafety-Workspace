// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AiSafetyIntelligence.Commands
//  Layer:          Tests (Unit)
//  File:           ActivateAiSafetyIntelligenceCommandHandlerTests.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Regression tests for the Phase 4 AiSafetyIntelligence full build-out.
//                  ActivateAiSafetyIntelligenceCommandHandler must forward the caller's ETag to
//                  IAiSafetyIntelligenceRepository.UpdateAsync, and must let a
//                  ConcurrencyConflictException raised by the repository propagate unchanged
//                  rather than swallowing it into a generic ApplicationError. Mirrors
//                  PlantSafety.Tests.Compliance.Auditing.Commands.StartAuditingCommandHandlerTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.AiSafetyIntelligence.Commands;
using PlantSafety.Domain.AiSafetyIntelligence.Repositories;
using PlantSafety.Domain.AiSafetyIntelligence.ValueObjects;
using AiSafetyIntelligenceAggregate = PlantSafety.Domain.AiSafetyIntelligence.Aggregates.AiSafetyIntelligence;

namespace PlantSafety.Tests.AiSafetyIntelligence.Commands;

public sealed class ActivateAiSafetyIntelligenceCommandHandlerTests : UnitTestBase
{
    private readonly IAiSafetyIntelligenceRepository _repo = Substitute.For<IAiSafetyIntelligenceRepository>();

    private ActivateAiSafetyIntelligenceCommandHandler Sut() => new(_repo, NullLogger<ActivateAiSafetyIntelligenceCommandHandler>());

    private static AiSafetyIntelligenceAggregate CreatedAiSafetyIntelligence() =>
        AiSafetyIntelligenceAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            insightType: "AnomalyDetection",
            description: "Unusual vibration pattern detected on Line 3 conveyor",
            confidenceScore: 0.87m,
            sourceModelName: "VibrationAnomalyModel-v3");

    private static ActivateAiSafetyIntelligenceCommand ActivateCommandFor(AiSafetyIntelligenceAggregate aggregate, string? eTag) =>
        new(
            CommandId: Guid.NewGuid(),
            CorrelationId: TestCorrelationId.Value,
            CausationId: TestCausationId.Value,
            TenantId: TestTenantId.Value,
            AiSafetyIntelligenceId: aggregate.AiSafetyIntelligenceId.Value,
            PerformedByUserId: TestUserId.Value)
        { ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ForwardsCallerEtagToRepository()
    {
        var aggregate = CreatedAiSafetyIntelligence();
        _repo.GetByIdAsync(Arg.Any<AiSafetyIntelligenceId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(aggregate);

        var command = ActivateCommandFor(aggregate, "0x0000000000005678");

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).UpdateAsync(aggregate, "0x0000000000005678", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var aggregate = CreatedAiSafetyIntelligence();
        _repo.GetByIdAsync(Arg.Any<AiSafetyIntelligenceId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(aggregate);
        _repo.UpdateAsync(Arg.Any<AiSafetyIntelligenceAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("AiSafetyIntelligence was modified by another process or the ETag is stale.", TestCorrelationId.Value));

        var command = ActivateCommandFor(aggregate, "stale-etag");

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() => Sut().Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenAiSafetyIntelligenceDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<AiSafetyIntelligenceId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((AiSafetyIntelligenceAggregate?)null);

        var command = ActivateCommandFor(CreatedAiSafetyIntelligence(), null);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AiSafetyIntelligence.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing AiSafetyIntelligence record must map to HTTP 404, not the default 500");
    }
}
