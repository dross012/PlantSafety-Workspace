// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.RiskManagement
//  Layer:          Tests (Unit)
//  File:           RiskAggregateTests.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code unit-testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Risk aggregate root: full 3-phase lifecycle (Create -> Score
//                  -> Prioritize -> ApplyControlMeasure -> ValidateEffectiveness -> StartReviewCycle
//                  -> CompleteReview -> Close -> Archive), invariant-guard tests for illegal
//                  transitions and the control-measure justification-required rule, and the
//                  RiskArchivedEvent added in a prior pass.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.RiskManagement.Aggregates;
using PlantSafety.Domain.RiskManagement.Enums;
using PlantSafety.Domain.RiskManagement.Events;
using PlantSafety.Domain.RiskManagement.Exceptions;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.Domain.RiskManagement;

/// <summary>Unit tests for <see cref="Risk"/> aggregate in the RiskManagement bounded context.</summary>
public sealed class RiskAggregateTests : UnitTestBase
{
    private static Risk CreateIdentified() =>
        Risk.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            riskTitle: "Unguarded conveyor pinch point",
            location: "Line 3",
            hazardSource: "Mechanical",
            ownerId: TestUserId);

    // ---- Create --------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnAggregate() => CreateIdentified().Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusIdentified()
    {
        var agg = CreateIdentified();
        agg.Status.Should().Be(RiskStatus.Identified);
    }

    [Fact]
    public void Create_ShouldRaiseRiskCreatedEvent()
    {
        var agg = CreateIdentified();
        agg.DomainEvents.ShouldRaiseSingleEvent<RiskCreatedEvent>();
    }

    [Fact]
    public void Create_TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = CreateIdentified();
        var a2 = CreateIdentified();
        a1.RiskId.Should().NotBe(a2.RiskId);
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmpty()
    {
        var agg = CreateIdentified();
        agg.ClearDomainEvents();
        agg.DomainEvents.Should().BeEmpty();
    }

    // ---- ScoreRisk -------------------------------------------------------------

    [Fact]
    public void ScoreRisk_WhenIdentified_ShouldTransitionToScored()
    {
        var agg = CreateIdentified();
        agg.ScoreRisk(4, 3, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(RiskStatus.Scored);
    }

    [Fact]
    public void ScoreRisk_ShouldDeriveCategoryFromScore()
    {
        var agg = CreateIdentified();
        agg.ScoreRisk(5, 5, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Category.Should().Be(RiskLevel.Critical);
    }

    [Fact]
    public void ScoreRisk_ShouldRaiseEvent()
    {
        var agg = CreateIdentified();
        agg.ScoreRisk(4, 3, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<RiskScoredEvent>();
    }

    [Fact]
    public void ScoreRisk_WhenAlreadyScored_ShouldAllowRescoring()
    {
        var agg = CreateIdentified();
        agg.ScoreRisk(4, 3, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        var act = () => agg.ScoreRisk(2, 2, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().NotThrow();
    }

    [Fact]
    public void ScoreRisk_WhenMitigated_ShouldThrowInvalidStatusTransition()
    {
        var agg = MitigateRisk();
        var act = () => agg.ScoreRisk(3, 3, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<RiskInvalidStatusTransitionException>();
    }

    [Fact]
    public void ScoreRisk_WithOutOfRangeSeverity_ShouldThrowArgumentOutOfRange()
    {
        var agg = CreateIdentified();
        var act = () => agg.ScoreRisk(6, 3, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ---- Prioritize --------------------------------------------------------------

    private static Risk CreateScored()
    {
        var agg = CreateIdentified();
        agg.ScoreRisk(4, 3, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return agg;
    }

    [Fact]
    public void Prioritize_WhenScored_ShouldTransitionToPrioritized()
    {
        var agg = CreateScored();
        agg.Prioritize(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(RiskStatus.Prioritized);
    }

    [Fact]
    public void Prioritize_WithNullPriority_ShouldDeriveFromRiskLevel()
    {
        var agg = CreateScored();
        agg.Prioritize(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Priority.Should().Be(RiskPriority.FromRiskLevel(agg.Category));
    }

    [Fact]
    public void Prioritize_WhenIdentified_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateIdentified();
        var act = () => agg.Prioritize(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<RiskInvalidStatusTransitionException>();
    }

    // ---- ApplyControlMeasure: Hierarchy-of-Controls justification invariant ----

    private static Risk CreatePrioritized()
    {
        var agg = CreateScored();
        agg.Prioritize(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return agg;
    }

    [Fact]
    public void ApplyControlMeasure_WithHighEffectivenessLevel_ShouldNotRequireJustification()
    {
        var agg = CreatePrioritized();
        var act = () => agg.ApplyControlMeasure(ControlHierarchyLevel.Elimination, "Remove hazard", null, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().NotThrow();
    }

    [Fact]
    public void ApplyControlMeasure_WithLowEffectivenessLevelAndNoJustification_ShouldThrowJustificationRequired()
    {
        var agg = CreatePrioritized();
        var act = () => agg.ApplyControlMeasure(ControlHierarchyLevel.Ppe, "Issue gloves", null, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<RiskControlJustificationRequiredException>();
    }

    [Fact]
    public void ApplyControlMeasure_WithLowEffectivenessLevelAndWhitespaceJustification_ShouldThrowJustificationRequired()
    {
        var agg = CreatePrioritized();
        var act = () => agg.ApplyControlMeasure(ControlHierarchyLevel.Ppe, "Issue gloves", "   ", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<RiskControlJustificationRequiredException>();
    }

    [Fact]
    public void ApplyControlMeasure_WithLowEffectivenessLevelAndJustification_ShouldSucceed()
    {
        var agg = CreatePrioritized();
        var measure = agg.ApplyControlMeasure(ControlHierarchyLevel.Ppe, "Issue gloves", "Elimination not feasible at this workstation", TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        measure.Justification.Should().Be("Elimination not feasible at this workstation");
    }

    [Fact]
    public void ApplyControlMeasure_ShouldTransitionToControlsInProgress()
    {
        var agg = CreatePrioritized();
        agg.ApplyControlMeasure(ControlHierarchyLevel.Elimination, "Remove hazard", null, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(RiskStatus.ControlsInProgress);
    }

    [Fact]
    public void ApplyControlMeasure_ShouldAddToControlMeasuresCollection()
    {
        var agg = CreatePrioritized();
        var measure = agg.ApplyControlMeasure(ControlHierarchyLevel.Elimination, "Remove hazard", null, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ControlMeasures.Should().ContainSingle(m => m.ControlMeasureId == measure.ControlMeasureId);
    }

    [Fact]
    public void ApplyControlMeasure_ShouldRaiseEvent()
    {
        var agg = CreatePrioritized();
        agg.ApplyControlMeasure(ControlHierarchyLevel.Elimination, "Remove hazard", null, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<RiskControlMeasureAppliedEvent>();
    }

    [Fact]
    public void ApplyControlMeasure_WhenIdentified_ShouldThrowInvalidOperationException()
    {
        var agg = CreateIdentified();
        var act = () => agg.ApplyControlMeasure(ControlHierarchyLevel.Elimination, "Remove hazard", null, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<InvalidOperationException>();
    }

    // ---- ValidateEffectiveness ---------------------------------------------------

    private static Risk CreateControlsInProgress()
    {
        var agg = CreatePrioritized();
        agg.ApplyControlMeasure(ControlHierarchyLevel.Elimination, "Remove hazard", null, TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return agg;
    }

    [Fact]
    public void ValidateEffectiveness_WithAcceptableResidualRisk_ShouldTransitionToMitigated()
    {
        var agg = CreateControlsInProgress();
        var isAcceptable = agg.ValidateEffectiveness(1, 1, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        isAcceptable.Should().BeTrue();
        agg.Status.Should().Be(RiskStatus.Mitigated);
    }

    [Fact]
    public void ValidateEffectiveness_WithUnacceptableResidualRisk_ShouldRemainControlsInProgress()
    {
        var agg = CreateControlsInProgress();
        var isAcceptable = agg.ValidateEffectiveness(5, 5, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        isAcceptable.Should().BeFalse();
        agg.Status.Should().Be(RiskStatus.ControlsInProgress);
    }

    [Fact]
    public void ValidateEffectiveness_ShouldRaiseEvent()
    {
        var agg = CreateControlsInProgress();
        agg.ValidateEffectiveness(1, 1, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<RiskEffectivenessValidatedEvent>();
    }

    [Fact]
    public void ValidateEffectiveness_WhenIdentified_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateIdentified();
        var act = () => agg.ValidateEffectiveness(1, 1, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<RiskInvalidStatusTransitionException>();
    }

    // ---- Phase 3: Review cycle -----------------------------------------------------

    private static Risk MitigateRisk()
    {
        var agg = CreateControlsInProgress();
        agg.ValidateEffectiveness(1, 1, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return agg;
    }

    [Fact]
    public void StartReviewCycle_WhenMitigated_ShouldTransitionToUnderReview()
    {
        var agg = MitigateRisk();
        agg.StartReviewCycle(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(RiskStatus.UnderReview);
    }

    [Fact]
    public void StartReviewCycle_WhenIdentified_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateIdentified();
        var act = () => agg.StartReviewCycle(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<RiskInvalidStatusTransitionException>();
    }

    [Fact]
    public void CompleteReview_WhenUnderReview_ShouldReturnToMitigatedAndBumpVersion()
    {
        var agg = MitigateRisk();
        var versionBefore = agg.RiskVersion;
        agg.StartReviewCycle(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.CompleteReview(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.Status.Should().Be(RiskStatus.Mitigated);
        agg.RiskVersion.Should().Be(versionBefore + 1);
    }

    [Fact]
    public void CompleteReview_ShouldIncrementReviewCount()
    {
        var agg = MitigateRisk();
        agg.StartReviewCycle(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.CompleteReview(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        agg.ReviewCount.Should().Be(1);
    }

    [Fact]
    public void CompleteReview_WhenNotUnderReview_ShouldThrowInvalidStatusTransition()
    {
        var agg = MitigateRisk();
        var act = () => agg.CompleteReview(null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<RiskInvalidStatusTransitionException>();
    }

    // ---- Close ------------------------------------------------------------------

    [Fact]
    public void Close_WhenMitigated_ShouldTransitionToClosed()
    {
        var agg = MitigateRisk();
        agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(RiskStatus.Closed);
    }

    [Fact]
    public void Close_WhenUnderReview_ShouldTransitionToClosed()
    {
        var agg = MitigateRisk();
        agg.StartReviewCycle(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(RiskStatus.Closed);
    }

    [Fact]
    public void Close_ShouldRaiseEvent()
    {
        var agg = MitigateRisk();
        agg.ClearDomainEvents();
        agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<RiskClosedEvent>();
    }

    [Fact]
    public void Close_WhenIdentified_ShouldThrowInvalidOperationException()
    {
        var agg = CreateIdentified();
        var act = () => agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<InvalidOperationException>();
    }

    // ---- Archive (domain event added in a prior pass) --------------------------

    private static Risk CreateClosed()
    {
        var agg = MitigateRisk();
        agg.Close(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ClearDomainEvents();
        return agg;
    }

    [Fact]
    public void Archive_WhenClosed_ShouldTransitionToArchived()
    {
        var agg = CreateClosed();
        agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(RiskStatus.Archived);
    }

    [Fact]
    public void Archive_WhenClosed_ShouldSetArchivedAtUtc()
    {
        var agg = CreateClosed();
        agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.ArchivedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void Archive_WhenClosed_ShouldRaiseRiskArchivedEvent()
    {
        var agg = CreateClosed();
        agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<RiskArchivedEvent>();
        evt.RiskId.Should().Be(agg.RiskId);
    }

    [Fact]
    public void Archive_ArchivedEvent_ShouldHaveSchemaVersion2026()
    {
        var agg = CreateClosed();
        agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<RiskArchivedEvent>();
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void Archive_WhenNotClosed_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateIdentified();
        var act = () => agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<RiskInvalidStatusTransitionException>();
    }
}
