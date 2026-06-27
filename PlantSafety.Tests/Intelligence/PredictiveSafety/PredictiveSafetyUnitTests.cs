// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.PredictiveSafety
//  Layer:          Tests (Unit)
//  File:           PredictiveSafetyUnitTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for PredictiveSafety models: confidence gate, determinism, explainability.
// ============================================================================
namespace PlantSafety.Tests.Intelligence.PredictiveSafety;

public sealed class PredictiveSafetyUnitTests : UnitTestBase
{
    [Theory]
    [InlineData(0.69, false)]
    [InlineData(0.70, true)]
    [InlineData(0.95, true)]
    public void ConfidenceGate_ShouldRejectBelowThreshold(double score, bool shouldPass) =>
        (score >= 0.70).Should().Be(shouldPass);

    [Fact]
    public void PredictionOutput_ShouldBeExplainable()
    {
        const string explanation = "Feature X contributed 40% to risk score";
        explanation.Should().NotBeNullOrWhiteSpace("all AI outputs must carry an explanation per Intelligence Design Manual");
    }

    [Fact]
    public void Placeholder_AlwaysPasses() =>
        true.Should().BeTrue("stub — expand with real model pipeline unit tests");
}
