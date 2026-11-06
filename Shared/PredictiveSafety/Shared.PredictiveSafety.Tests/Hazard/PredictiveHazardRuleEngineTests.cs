// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveHazardRuleEngineTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Severity;
using Shared.PredictiveSafety.Abstractions.Hazard;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Hazard;

public sealed class PredictiveHazardRuleEngineTests
{
    private static PredictiveHazardSignals BaseSignals() => new(
        EventType: "ChemicalSpill", Description: null, EmployeeExposureCount: 0, MachineFailureCount: 0, EventChainLength: 0,
        RoutingTriggeredAuditCount: 0, RoutingChangedComplianceElementCount: 0, SchedulingRequiresImmediateAction: false, RecentReliabilityFailureCount: 0);

    [Fact]
    public void Evaluate_NoTriggers_NoEscalation()
    {
        PredictiveHazardRuleEngine.Evaluate(BaseSignals(), SeverityLevel.Minor).RequiresEscalation.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_SevereOrAbove_Escalates()
    {
        var outcome = PredictiveHazardRuleEngine.Evaluate(BaseSignals(), SeverityLevel.Severe);

        outcome.RequiresEscalation.Should().BeTrue();
        outcome.AppliedRuleCodes.Should().Contain("PREDICTED-SEVERITY-ESCALATION");
    }

    [Fact]
    public void Evaluate_RoutingAuditTrigger_Escalates()
    {
        PredictiveHazardSignals signals = BaseSignals() with { RoutingTriggeredAuditCount = 1 };

        PredictiveHazardRuleEngine.Evaluate(signals, SeverityLevel.Minor).AppliedRuleCodes.Should().Contain("ROUTING-AUDIT-TRIGGER-ESCALATION");
    }

    [Fact]
    public void Evaluate_SchedulingImmediateAction_Escalates()
    {
        PredictiveHazardSignals signals = BaseSignals() with { SchedulingRequiresImmediateAction = true };

        PredictiveHazardRuleEngine.Evaluate(signals, SeverityLevel.Minor).AppliedRuleCodes.Should().Contain("SCHEDULING-IMMEDIATE-ACTION-ESCALATION");
    }

    [Fact]
    public void Evaluate_HighReliabilityFailureCount_Escalates()
    {
        PredictiveHazardSignals signals = BaseSignals() with { RecentReliabilityFailureCount = 3 };

        PredictiveHazardRuleEngine.Evaluate(signals, SeverityLevel.Minor).AppliedRuleCodes.Should().Contain("RELIABILITY-SIGNAL-ESCALATION");
    }

    [Fact]
    public void Evaluate_SameInputTwice_ProducesIdenticalResult()
    {
        PredictiveHazardSignals signals = BaseSignals() with { RoutingTriggeredAuditCount = 1 };

        PredictiveHazardRuleEngine.Evaluate(signals, SeverityLevel.Moderate)
            .Should().BeEquivalentTo(PredictiveHazardRuleEngine.Evaluate(signals, SeverityLevel.Moderate));
    }

    [Fact]
    public void Evaluate_NullSignals_Throws()
    {
        Action act = () => PredictiveHazardRuleEngine.Evaluate(null!, SeverityLevel.Minor);

        act.Should().Throw<ArgumentNullException>();
    }
}
