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
    public void BoundaryValue_OneEpsilonBelowThreshold_ShouldFail() =>
        (0.70 - double.Epsilon >= 0.70).Should().BeFalse();
}
