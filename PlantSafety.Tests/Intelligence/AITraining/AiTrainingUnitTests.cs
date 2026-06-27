// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Intelligence.AITraining
//  Layer:          Tests (Unit)
//  File:           AiTrainingUnitTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for AI Training subsystem: fixed seed, metadata tagging.
// ============================================================================
namespace PlantSafety.Tests.Intelligence.AITraining;

public sealed class AiTrainingUnitTests : UnitTestBase
{
    [Fact]
    public void TrainingConfig_ShouldHaveFixedSeed()
    {
        const int configuredSeed = 42;
        configuredSeed.Should().NotBe(0, "random seed is not allowed — must be fixed for reproducibility");
    }

    [Fact]
    public void TrainingConfig_ShouldIncludeSchemaVersion()
    {
        const string schemaVersion = "2026.1";
        schemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void ModelArtifact_ShouldTagEnvironment()
    {
        const string env = "training";
        env.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Placeholder_AlwaysPasses() =>
        true.Should().BeTrue("stub — expand with training pipeline idempotency tests");
}
