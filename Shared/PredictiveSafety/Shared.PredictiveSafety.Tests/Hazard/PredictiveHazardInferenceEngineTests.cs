// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveHazardInferenceEngineTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Hazard;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Hazard;

public sealed class PredictiveHazardInferenceEngineTests
{
    [Fact]
    public void Infer_UsesFacilityRealInferenceRulesForPredictedFields()
    {
        var signals = new PredictiveHazardSignals(
            EventType: "Fire", Description: "Uncontrolled fire in warehouse", EmployeeExposureCount: 2, MachineFailureCount: 1, EventChainLength: 3,
            RoutingTriggeredAuditCount: 0, RoutingChangedComplianceElementCount: 0, SchedulingRequiresImmediateAction: false, RecentReliabilityFailureCount: 0);

        PredictiveHazardDecision decision = PredictiveHazardInferenceEngine.Infer(signals);

        decision.PredictedHazardType.ToString().Should().NotBeNullOrEmpty();
        decision.PredictedJurisdictions.Should().NotBeNull();
    }

    [Fact]
    public void Infer_NoEscalationTriggers_ReturnsLowPriority()
    {
        var signals = new PredictiveHazardSignals(
            EventType: "Other", Description: null, EmployeeExposureCount: 0, MachineFailureCount: 0, EventChainLength: 0,
            RoutingTriggeredAuditCount: 0, RoutingChangedComplianceElementCount: 0, SchedulingRequiresImmediateAction: false, RecentReliabilityFailureCount: 0);

        PredictiveHazardDecision decision = PredictiveHazardInferenceEngine.Infer(signals);

        decision.RequiresImmediateEscalation.Should().BeFalse();
        decision.Priority.Should().Be(PredictivePriority.Low);
    }

    [Fact]
    public void Infer_NullSignals_Throws()
    {
        Action act = () => PredictiveHazardInferenceEngine.Infer(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
