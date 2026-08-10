// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveEscalationClassifierTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.PredictiveSafety.Abstractions.Common;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Common;

public sealed class PredictiveEscalationClassifierTests
{
    [Theory]
    [InlineData(0, PredictivePriority.Low)]
    [InlineData(1, PredictivePriority.Medium)]
    [InlineData(2, PredictivePriority.High)]
    [InlineData(3, PredictivePriority.Critical)]
    [InlineData(5, PredictivePriority.Critical)]
    public void Classify_RuleCodeCount_MapsToExpectedPriority(int ruleCodeCount, PredictivePriority expected)
    {
        var codes = new List<string>();
        for (int i = 0; i < ruleCodeCount; i++)
        {
            codes.Add($"RULE-{i}");
        }

        var outcome = new PredictiveEscalationOutcome(ruleCodeCount > 0, codes);

        PredictiveEscalationClassifier.Classify(outcome).Should().Be(expected);
    }

    [Fact]
    public void Classify_NullOutcome_Throws()
    {
        Action act = () => PredictiveEscalationClassifier.Classify(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
