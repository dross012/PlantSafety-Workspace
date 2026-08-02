// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.RiskManagement
//  Layer:          Tests (Unit)
//  File:           RiskAssessmentCommandHandlersTests.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code unit-testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the seven 3-phase Risk Management workflow command handlers
//                  (ScoreRiskCommandHandler, PrioritizeRiskCommandHandler,
//                  ApplyRiskControlMeasureCommandHandler, ValidateRiskEffectivenessCommandHandler,
//                  StartRiskReviewCycleCommandHandler, CompleteRiskReviewCommandHandler,
//                  CloseRiskCommandHandler) with a mocked IRiskRepository.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.RiskManagement.Commands;
using PlantSafety.Domain.RiskManagement.Enums;
using PlantSafety.Domain.RiskManagement.Interfaces;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;
using RiskAggregate = PlantSafety.Domain.RiskManagement.Aggregates.Risk;

namespace PlantSafety.Tests.Application.RiskManagement;

public sealed class RiskAssessmentCommandHandlersTests : UnitTestBase
{
    private readonly IRiskRepository _repository = Substitute.For<IRiskRepository>();

    private static RiskAggregate NewIdentifiedRisk() =>
        RiskAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, riskTitle: "Unguarded conveyor", location: "Line 3", hazardSource: "Mechanical", ownerId: TestUserId);

    private void ReturnsRisk(RiskAggregate risk) =>
        _repository.GetByIdAsync(Arg.Any<RiskId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(risk);

    // ---- ScoreRiskCommandHandler -------------------------------------------------

    [Fact]
    public async Task ScoreRisk_WhenIdentified_ShouldTransitionToScored()
    {
        var risk = NewIdentifiedRisk();
        ReturnsRisk(risk);

        var handler = new ScoreRiskCommandHandler(_repository, NullLogger<ScoreRiskCommandHandler>());
        var cmd = new ScoreRiskCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, risk.RiskId.Value, 4, 3, TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Scored");
        await _repository.Received(1).UpdateAsync(risk, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScoreRisk_WhenRiskNotFound_ShouldReturnFailure()
    {
        _repository.GetByIdAsync(Arg.Any<RiskId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((RiskAggregate?)null);

        var handler = new ScoreRiskCommandHandler(_repository, NullLogger<ScoreRiskCommandHandler>());
        var cmd = new ScoreRiskCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, Guid.NewGuid(), 4, 3, TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("RiskManagement.Risk.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Risk must map to HTTP 404, not the default 500");
    }

    // ---- PrioritizeRiskCommandHandler --------------------------------------------

    private static RiskAggregate NewScoredRisk()
    {
        var risk = NewIdentifiedRisk();
        risk.ScoreRisk(4, 3, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return risk;
    }

    [Fact]
    public async Task Prioritize_WhenScored_ShouldTransitionToPrioritized()
    {
        var risk = NewScoredRisk();
        ReturnsRisk(risk);

        var handler = new PrioritizeRiskCommandHandler(_repository, NullLogger<PrioritizeRiskCommandHandler>());
        var cmd = new PrioritizeRiskCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, risk.RiskId.Value, null, TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Prioritized");
    }

    // ---- ApplyRiskControlMeasureCommandHandler -----------------------------------

    private static RiskAggregate NewPrioritizedRisk()
    {
        var risk = NewScoredRisk();
        risk.Prioritize(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return risk;
    }

    [Fact]
    public async Task ApplyControlMeasure_WithHighEffectivenessLevel_ShouldSucceed()
    {
        var risk = NewPrioritizedRisk();
        ReturnsRisk(risk);

        var handler = new ApplyRiskControlMeasureCommandHandler(_repository, NullLogger<ApplyRiskControlMeasureCommandHandler>());
        var cmd = new ApplyRiskControlMeasureCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, risk.RiskId.Value, "Elimination", "Remove hazard", null, TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("ControlsInProgress");
        result.Value.ControlMeasureId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task ApplyControlMeasure_WithLowEffectivenessLevelAndNoJustification_ShouldReturnFailure()
    {
        var risk = NewPrioritizedRisk();
        ReturnsRisk(risk);

        var handler = new ApplyRiskControlMeasureCommandHandler(_repository, NullLogger<ApplyRiskControlMeasureCommandHandler>());
        var cmd = new ApplyRiskControlMeasureCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, risk.RiskId.Value, "Ppe", "Issue gloves", null, TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("RiskManagement.Risk.ApplyControlMeasureFailed");
    }

    // ---- ValidateRiskEffectivenessCommandHandler ---------------------------------

    private static RiskAggregate NewControlsInProgressRisk()
    {
        var risk = NewPrioritizedRisk();
        risk.ApplyControlMeasure(ControlHierarchyLevel.Elimination, "Remove hazard", null, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return risk;
    }

    [Fact]
    public async Task ValidateEffectiveness_WithAcceptableResidualRisk_ShouldReturnAcceptableTrue()
    {
        var risk = NewControlsInProgressRisk();
        ReturnsRisk(risk);

        var handler = new ValidateRiskEffectivenessCommandHandler(_repository, NullLogger<ValidateRiskEffectivenessCommandHandler>());
        var cmd = new ValidateRiskEffectivenessCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, risk.RiskId.Value, 1, 1, TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsAcceptable.Should().BeTrue();
        result.Value.Status.Should().Be("Mitigated");
    }

    [Fact]
    public async Task ValidateEffectiveness_WithUnacceptableResidualRisk_ShouldReturnAcceptableFalse()
    {
        var risk = NewControlsInProgressRisk();
        ReturnsRisk(risk);

        var handler = new ValidateRiskEffectivenessCommandHandler(_repository, NullLogger<ValidateRiskEffectivenessCommandHandler>());
        var cmd = new ValidateRiskEffectivenessCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, risk.RiskId.Value, 5, 5, TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsAcceptable.Should().BeFalse();
        result.Value.Status.Should().Be("ControlsInProgress");
    }

    // ---- StartRiskReviewCycleCommandHandler / CompleteRiskReviewCommandHandler --

    private static RiskAggregate NewMitigatedRisk()
    {
        var risk = NewControlsInProgressRisk();
        risk.ValidateEffectiveness(1, 1, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return risk;
    }

    [Fact]
    public async Task StartReviewCycle_WhenMitigated_ShouldTransitionToUnderReview()
    {
        var risk = NewMitigatedRisk();
        ReturnsRisk(risk);

        var handler = new StartRiskReviewCycleCommandHandler(_repository, NullLogger<StartRiskReviewCycleCommandHandler>());
        var cmd = new StartRiskReviewCycleCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, risk.RiskId.Value, TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("UnderReview");
    }

    [Fact]
    public async Task CompleteReview_WhenUnderReview_ShouldReturnToMitigated()
    {
        var risk = NewMitigatedRisk();
        risk.StartReviewCycle(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        ReturnsRisk(risk);

        var handler = new CompleteRiskReviewCommandHandler(_repository, NullLogger<CompleteRiskReviewCommandHandler>());
        var cmd = new CompleteRiskReviewCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, risk.RiskId.Value, null, TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Mitigated");
    }

    // ---- CloseRiskCommandHandler --------------------------------------------------

    [Fact]
    public async Task Close_WhenMitigated_ShouldTransitionToClosed()
    {
        var risk = NewMitigatedRisk();
        ReturnsRisk(risk);

        var handler = new CloseRiskCommandHandler(_repository, NullLogger<CloseRiskCommandHandler>());
        var cmd = new CloseRiskCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, risk.RiskId.Value, TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Closed");
    }

    [Fact]
    public async Task Close_WhenRiskNotFound_ShouldReturnFailure()
    {
        _repository.GetByIdAsync(Arg.Any<RiskId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((RiskAggregate?)null);

        var handler = new CloseRiskCommandHandler(_repository, NullLogger<CloseRiskCommandHandler>());
        var cmd = new CloseRiskCommand(Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, FixedUtcNow,
            TestTenantId.Value, Guid.NewGuid(), TestUserId.Value);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("RiskManagement.Risk.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing Risk must map to HTTP 404, not the default 500");
    }
}
