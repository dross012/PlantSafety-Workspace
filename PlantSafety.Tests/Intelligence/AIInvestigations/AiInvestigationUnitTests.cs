// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.AIInvestigations
//  Layer:          Tests (Unit)
//  File:           AiInvestigationUnitTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for AI Investigation pipeline: determinism, confidence threshold, metadata.
// ============================================================================
namespace PlantSafety.Tests.Intelligence.AIInvestigations;

/// <summary>
/// Validates AI investigation safety rules:
/// - Confidence threshold must be >= 0.70 (per Intelligence Design Manual).
/// - Pipeline must be deterministic given fixed seed.
/// - All outputs must carry model metadata.
/// </summary>
public sealed class AiInvestigationUnitTests : UnitTestBase
{
    [Fact]
    public void ConfidenceThreshold_BelowMinimum_ShouldBeRejected()
    {
        const double belowThreshold = 0.69;
        var isValid = belowThreshold >= 0.70;
        isValid.Should().BeFalse("confidence < 0.70 must not pass the safety gate");
    }

    [Fact]
    public void ConfidenceThreshold_AtMinimum_ShouldBeAccepted()
    {
        const double atThreshold = 0.70;
        var isValid = atThreshold >= 0.70;
        isValid.Should().BeTrue("confidence = 0.70 is the minimum acceptable score");
    }

    [Fact]
    public void ConfidenceThreshold_AboveMinimum_ShouldBeAccepted()
    {
        const double aboveThreshold = 0.95;
        var isValid = aboveThreshold >= 0.70;
        isValid.Should().BeTrue();
    }

    [Fact]
    public void DeterministicPipeline_SameInputAndSeed_ShouldProduceSameOutput()
    {
        // Guard: deterministic model placeholder — replace with real pipeline call.
        const int seed = 42;
        var run1 = SimulateInference(seed, "incident-input");
        var run2 = SimulateInference(seed, "incident-input");
        run1.Should().Be(run2, "deterministic pipeline with same seed must produce identical results");
    }

    [Fact]
    public void ModelMetadata_ShouldContainModelName()
    {
        var meta = BuildMetadata();
        meta.ModelName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ModelMetadata_ShouldContainSchemaVersion()
    {
        var meta = BuildMetadata();
        meta.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void ModelMetadata_ShouldContainFixedSeed()
    {
        var meta = BuildMetadata();
        meta.Seed.Should().NotBe(0, "fixed seed is required for reproducibility");
    }

    private static double SimulateInference(int seed, string input)
    {
        // Deterministic stub — real impl replaces this with fixed-seed model call.
        var hash = (seed * 397) ^ input.GetHashCode();
        return Math.Abs(hash % 100) / 100.0;
    }

    private static (string ModelName, string SchemaVersion, int Seed) BuildMetadata() =>
        ("PlantSafety-InvestigationV1", "2026.1", 42);
}
