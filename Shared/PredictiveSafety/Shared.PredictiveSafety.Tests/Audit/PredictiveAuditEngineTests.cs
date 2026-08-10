// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveAuditEngineTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.PredictiveSafety.Abstractions.Audit;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Audit;

public sealed class PredictiveAuditEngineTests
{
    private static PredictiveAuditSignals BaseSignals() => new(
        "corr-1", TotalLogEventCount: 0, WarningOrAboveLogEventCount: 0, DistinctEventIdCount: 0, GovernanceViolationCount: 0);

    [Fact]
    public void Evaluate_NoTriggers_NoAnomaly()
    {
        PredictiveAuditRuleEngine.Evaluate(BaseSignals()).RequiresEscalation.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_ElevatedWarningVolume_DetectsAnomaly()
    {
        PredictiveAuditRuleEngine.Evaluate(BaseSignals() with { WarningOrAboveLogEventCount = 5 })
            .AppliedRuleCodes.Should().Contain("ELEVATED-WARNING-VOLUME");
    }

    [Fact]
    public void Evaluate_GovernanceViolationPresent_DetectsAnomaly()
    {
        PredictiveAuditRuleEngine.Evaluate(BaseSignals() with { GovernanceViolationCount = 1 })
            .AppliedRuleCodes.Should().Contain("GOVERNANCE-VIOLATION-PRESENT");
    }

    [Fact]
    public void Evaluate_RepetitiveEventPattern_DetectsAnomaly()
    {
        PredictiveAuditRuleEngine.Evaluate(BaseSignals() with { DistinctEventIdCount = 1, TotalLogEventCount = 10 })
            .AppliedRuleCodes.Should().Contain("REPETITIVE-EVENT-PATTERN");
    }

    [Fact]
    public void Evaluate_ManyDistinctEvents_NoRepetitivePatternRule()
    {
        PredictiveAuditRuleEngine.Evaluate(BaseSignals() with { DistinctEventIdCount = 10, TotalLogEventCount = 10 })
            .AppliedRuleCodes.Should().NotContain("REPETITIVE-EVENT-PATTERN");
    }

    [Fact]
    public void Infer_NullSignals_Throws()
    {
        Action act = () => PredictiveAuditInferenceEngine.Infer(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
