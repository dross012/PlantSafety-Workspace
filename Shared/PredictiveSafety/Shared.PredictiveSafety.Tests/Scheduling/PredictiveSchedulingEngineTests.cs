// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveSchedulingEngineTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.PredictiveSafety.Abstractions.Scheduling;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Scheduling;

public sealed class PredictiveSchedulingEngineTests
{
    private static PredictiveSchedulingSignals BaseSignals() => new(
        TotalRequiredActionCount: 0, WorstCaseDueWithinHours: int.MaxValue,
        HazardPredictionRequiresEscalation: false, MaintenancePredictionRequiresImmediate: false, RoutingPredictionRequiresPriority: false);

    [Fact]
    public void Evaluate_NoTriggers_NoEscalation()
    {
        PredictiveSchedulingRuleEngine.Evaluate(BaseSignals()).RequiresEscalation.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_NearTermDue_Escalates()
    {
        PredictiveSchedulingRuleEngine.Evaluate(BaseSignals() with { WorstCaseDueWithinHours = 10 })
            .AppliedRuleCodes.Should().Contain("NEAR-TERM-DUE-ESCALATION");
    }

    [Fact]
    public void Evaluate_HighActionVolume_Escalates()
    {
        PredictiveSchedulingRuleEngine.Evaluate(BaseSignals() with { TotalRequiredActionCount = 3 })
            .AppliedRuleCodes.Should().Contain("ACTION-VOLUME-ESCALATION");
    }

    [Fact]
    public void Evaluate_UpstreamPredictionsPropagate()
    {
        PredictiveSchedulingSignals signals = BaseSignals() with
        {
            HazardPredictionRequiresEscalation = true,
            MaintenancePredictionRequiresImmediate = true,
            RoutingPredictionRequiresPriority = true,
        };

        var outcome = PredictiveSchedulingRuleEngine.Evaluate(signals);

        outcome.AppliedRuleCodes.Should().Contain("HAZARD-PREDICTION-ESCALATION");
        outcome.AppliedRuleCodes.Should().Contain("MAINTENANCE-PREDICTION-ESCALATION");
        outcome.AppliedRuleCodes.Should().Contain("ROUTING-PREDICTION-ESCALATION");
    }

    [Fact]
    public void Infer_NullSignals_Throws()
    {
        Action act = () => PredictiveSchedulingInferenceEngine.Infer(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
