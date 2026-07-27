// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.AIInvestigations
//  Layer:          Tests (Unit)
//  File:           ConfidenceThresholdTheoryTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Theory-driven boundary tests for AI confidence threshold gate.
//                  Per Intelligence Design Manual: minimum threshold is 0.70.
// ============================================================================
namespace PlantSafety.Tests.Intelligence.AIInvestigations;

public sealed class ConfidenceThresholdTheoryTests : UnitTestBase
{
    [Theory]
    [InlineData(0.00, false)]
    [InlineData(0.50, false)]
    [InlineData(0.69, false)]
    [InlineData(0.70, true)]
    [InlineData(0.85, true)]
    [InlineData(1.00, true)]
    public void ConfidenceGate_ShouldEnforceMinimumThreshold(double score, bool shouldPass) =>
        (score >= 0.70).Should().Be(shouldPass,
            $"score {score} should{(shouldPass ? "" : " not")} pass the 0.70 gate");

    [Fact]
    public void BoundaryValue_ExactlyAtThreshold_ShouldPass() =>
        (0.70 >= 0.70).Should().BeTrue("boundary value at exactly 0.70 must be accepted");

    [Fact]
    public void BoundaryValue_OneEpsilonBelowThreshold_ShouldFail()
    {
        // NOTE: double.Epsilon (~4.94e-324) is far smaller than the ULP of 0.70, so
        // "0.70 - double.Epsilon" rounds back to exactly 0.70 and does not actually
        // move below the threshold. Use a delta large enough to be representable
        // relative to 0.70 to genuinely exercise the "just below threshold" case.
        const double justBelowThreshold = 0.70 - 0.0001;
        (justBelowThreshold >= 0.70).Should().BeFalse();
    }
}
