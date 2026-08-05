// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Aggregates
//  Layer:          Tests (Unit)
//  File:           CognitiveRiskPredictionTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the CognitiveRiskPrediction aggregate root:
//                   Generate (including argument-validation guards), Acknowledge, Dismiss, and
//                   their guarded lifecycle transitions. Isolated, deterministic, <50ms per test.
//                   No I/O, no network.
// ============================================================================
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Events;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Exceptions;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Aggregates;

public sealed class CognitiveRiskPredictionTests : UnitTestBase
{
    private static CognitiveRiskPrediction GeneratePrediction(
        CognitiveRiskLevel level = CognitiveRiskLevel.Elevated, decimal score = 0.65m) =>
        CognitiveRiskPrediction.Generate(
            generatedBy: TestUserId,
            generatedAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: Guid.NewGuid(),
            workerUserId: TestUserId,
            sourceLedgerId: CognitiveLoadLedgerId.New(),
            predictedRiskLevel: level,
            predictionScore: score,
            modelVersion: "v1.0.0",
            predictionBasis: "MeanCognitiveLoadScore=70.00; OverloadEventFrequencyPercent=30.00");

    [Fact]
    public void Generate_WithValidInputs_ProducesGeneratedPredictionAndRaisesEvent()
    {
        var prediction = GeneratePrediction();

        prediction.Status.Should().Be(CognitiveRiskPredictionStatus.Generated);
        prediction.ModelVersion.Should().Be("v1.0.0");
        prediction.AcknowledgedAtUtc.Should().BeNull();
        prediction.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<CognitiveRiskPredictionGeneratedEvent>();
    }

    [Fact]
    public void Generate_TrimsModelVersionAndPredictionBasis()
    {
        var prediction = CognitiveRiskPrediction.Generate(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, TestUserId, CognitiveLoadLedgerId.New(),
            CognitiveRiskLevel.Low, 0.1m, "  v2  ", "  basis  ");

        prediction.ModelVersion.Should().Be("v2");
        prediction.PredictionBasis.Should().Be("basis");
    }

    [Fact]
    public void Generate_WithEmptyTenantId_ThrowsArgumentException()
    {
        var act = () => CognitiveRiskPrediction.Generate(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: Guid.Empty, siteId: null, workerUserId: TestUserId, sourceLedgerId: CognitiveLoadLedgerId.New(),
            predictedRiskLevel: CognitiveRiskLevel.Low, predictionScore: 0.1m, modelVersion: "v1", predictionBasis: "b");

        act.Should().Throw<ArgumentException>().WithParameterName("tenantId");
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Generate_WithPredictionScoreOutOfRange_ThrowsArgumentOutOfRangeException(decimal score)
    {
        var act = () => CognitiveRiskPrediction.Generate(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, TestUserId, CognitiveLoadLedgerId.New(),
            CognitiveRiskLevel.Low, score, "v1", "b");

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("predictionScore");
    }

    [Fact]
    public void Generate_WithBlankModelVersion_ThrowsArgumentException()
    {
        var act = () => CognitiveRiskPrediction.Generate(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, TestUserId, CognitiveLoadLedgerId.New(),
            CognitiveRiskLevel.Low, 0.1m, "   ", "b");

        act.Should().Throw<ArgumentException>().WithParameterName("modelVersion");
    }

    [Fact]
    public void Acknowledge_WhenGenerated_TransitionsToAcknowledgedAndRaisesEvent()
    {
        var prediction = GeneratePrediction();
        prediction.ClearDomainEvents();

        prediction.Acknowledge(TestUserId, "Reviewed, coaching scheduled.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        prediction.Status.Should().Be(CognitiveRiskPredictionStatus.Acknowledged);
        prediction.AcknowledgedBy.Should().Be(TestUserId);
        prediction.AcknowledgedAtUtc.Should().Be(FixedTimestamp);
        prediction.AcknowledgementNotes.Should().Be("Reviewed, coaching scheduled.");
        prediction.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<CognitiveRiskAlertAcknowledgedEvent>();
    }

    [Fact]
    public void Acknowledge_WithNullOrWhitespaceNotes_StoresNullNotes()
    {
        var prediction = GeneratePrediction();

        prediction.Acknowledge(TestUserId, "   ", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        prediction.AcknowledgementNotes.Should().BeNull();
    }

    [Fact]
    public void Acknowledge_WhenAlreadyAcknowledged_ThrowsInvalidStatusTransitionException()
    {
        var prediction = GeneratePrediction();
        prediction.Acknowledge(TestUserId, "first", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => prediction.Acknowledge(TestUserId, "second", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CognitiveRiskPredictionInvalidStatusTransitionException>();
    }

    [Fact]
    public void Dismiss_WhenGenerated_TransitionsToDismissed()
    {
        var prediction = GeneratePrediction();

        prediction.Dismiss(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        prediction.Status.Should().Be(CognitiveRiskPredictionStatus.Dismissed);
    }

    [Fact]
    public void Dismiss_WhenAlreadyAcknowledged_ThrowsInvalidStatusTransitionException()
    {
        var prediction = GeneratePrediction();
        prediction.Acknowledge(TestUserId, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => prediction.Dismiss(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CognitiveRiskPredictionInvalidStatusTransitionException>();
    }

    [Fact]
    public void Dismiss_WhenAlreadyDismissed_ThrowsInvalidStatusTransitionException()
    {
        var prediction = GeneratePrediction();
        prediction.Dismiss(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => prediction.Dismiss(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CognitiveRiskPredictionInvalidStatusTransitionException>();
    }
}
