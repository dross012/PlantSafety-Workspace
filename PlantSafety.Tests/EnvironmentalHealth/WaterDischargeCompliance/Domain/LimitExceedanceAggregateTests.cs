// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.WaterDischargeCompliance.Domain
//  Layer:          Tests (Unit)
//  File:           LimitExceedanceAggregateTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the LimitExceedance aggregate root, including the
//                  Detected -> ResponseInitiated -> Closed response workflow and the
//                  RequiresElevatedInspectionFrequency risk hook.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Enums;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Exceptions;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Rules;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Aggregates.LimitExceedance;

namespace PlantSafety.Tests.EnvironmentalHealth.WaterDischargeCompliance.Domain;

public sealed class LimitExceedanceAggregateTests : UnitTestBase
{
    private static Agg DetectedExceedance(int repeatExceedanceCount = 0) => Agg.Create(
        createdBy: TestUserId,
        createdAtUtc: FixedTimestamp,
        correlationId: TestCorrelationId,
        causationId: TestCausationId,
        actor: TestActor,
        tenantId: TestTenantId.Value,
        siteId: Guid.NewGuid(),
        dischargePointId: Guid.NewGuid(),
        effluentSampleId: Guid.NewGuid(),
        dischargeLimitId: Guid.NewGuid(),
        parameterName: "pH",
        measuredValue: 10.5m,
        limitValue: 9.0m,
        severity: ExceedanceSeverity.Major,
        detectedAtUtc: FixedTimestamp,
        repeatExceedanceCount: repeatExceedanceCount);

    [Fact]
    public void Create_WithValidData_ComputesExceedancePercentageAndRaisesEvent()
    {
        var exceedance = DetectedExceedance();

        exceedance.Status.Should().Be(LimitExceedanceStatus.Detected);
        exceedance.ExceedancePercentage.Should().Be(16.67m); // (10.5 - 9.0) / 9.0 * 100, rounded to 2dp
        exceedance.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void Create_WithZeroLimitValue_DoesNotDivideByZero()
    {
        var exceedance = Agg.Create(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
            causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value,
            measuredValue: 5m, limitValue: 0m, detectedAtUtc: FixedTimestamp);

        exceedance.ExceedancePercentage.Should().Be(0m);
    }

    [Fact]
    public void InitiateResponse_WhenDetected_TransitionsToResponseInitiated()
    {
        var exceedance = DetectedExceedance();

        exceedance.InitiateResponse(TestUserId, "Investigating root cause.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        exceedance.Status.Should().Be(LimitExceedanceStatus.ResponseInitiated);
        exceedance.ResponseInitiatedByUserId.Should().Be(TestUserId);
        exceedance.ResponseNotes.Should().Be("Investigating root cause.");
    }

    [Fact]
    public void InitiateResponse_WhenAlreadyInitiated_ThrowsInvalidStatusTransition()
    {
        var exceedance = DetectedExceedance();
        exceedance.InitiateResponse(TestUserId, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => exceedance.InitiateResponse(TestUserId, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<LimitExceedanceInvalidStatusTransitionException>();
    }

    [Fact]
    public void CloseResponse_WhenResponseInitiated_TransitionsToClosed()
    {
        var exceedance = DetectedExceedance();
        exceedance.InitiateResponse(TestUserId, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        exceedance.CloseResponse(TestUserId, "Root cause corrected; resampled within limit.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        exceedance.Status.Should().Be(LimitExceedanceStatus.Closed);
        exceedance.ClosedByUserId.Should().Be(TestUserId);
        exceedance.ResolutionNotes.Should().Be("Root cause corrected; resampled within limit.");
    }

    [Fact]
    public void CloseResponse_WhenStillDetected_ThrowsInvalidStatusTransition()
    {
        var exceedance = DetectedExceedance();

        var act = () => exceedance.CloseResponse(TestUserId, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<LimitExceedanceInvalidStatusTransitionException>();
    }

    [Fact]
    public void CloseResponse_WhenAlreadyClosed_ThrowsInvalidStatusTransition()
    {
        var exceedance = DetectedExceedance();
        exceedance.InitiateResponse(TestUserId, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        exceedance.CloseResponse(TestUserId, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => exceedance.CloseResponse(TestUserId, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<LimitExceedanceInvalidStatusTransitionException>();
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(5, true)]
    public void RequiresElevatedInspectionFrequency_ReflectsRepeatExceedanceCountThreshold(int repeatCount, bool expected)
    {
        LimitExceedanceLifecycleRules.RequiresElevatedInspectionFrequency(repeatCount).Should().Be(expected);
    }
}
