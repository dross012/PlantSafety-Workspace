// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveRoutingEngineTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.PredictiveSafety.Abstractions.Routing;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Routing;

public sealed class PredictiveRoutingEngineTests
{
    private static readonly Guid FacilityId = Guid.NewGuid();

    private static PredictiveRoutingSignals BaseSignals() => new(
        "ChemicalSpill", FacilityId, TriggeredAuditCount: 0, ChangedComplianceElementCount: 0,
        HazardPredictionRequiresEscalation: false, MaintenancePredictionRequiresImmediate: false);

    [Fact]
    public void Evaluate_NoTriggers_NoEscalation()
    {
        PredictiveRoutingRuleEngine.Evaluate(BaseSignals()).RequiresEscalation.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_AuditTriggered_Escalates()
    {
        PredictiveRoutingRuleEngine.Evaluate(BaseSignals() with { TriggeredAuditCount = 1 })
            .AppliedRuleCodes.Should().Contain("AUDIT-TRIGGER-ESCALATION");
    }

    [Fact]
    public void Evaluate_HazardPredictionEscalation_Propagates()
    {
        PredictiveRoutingRuleEngine.Evaluate(BaseSignals() with { HazardPredictionRequiresEscalation = true })
            .AppliedRuleCodes.Should().Contain("HAZARD-PREDICTION-ESCALATION");
    }

    [Fact]
    public void Infer_PreservesEventCodeAndFacilityId()
    {
        PredictiveRoutingDecision decision = PredictiveRoutingInferenceEngine.Infer(BaseSignals());

        decision.EventCode.Should().Be("ChemicalSpill");
        decision.FacilityId.Should().Be(FacilityId);
    }

    [Fact]
    public void Infer_NullSignals_Throws()
    {
        Action act = () => PredictiveRoutingInferenceEngine.Infer(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
