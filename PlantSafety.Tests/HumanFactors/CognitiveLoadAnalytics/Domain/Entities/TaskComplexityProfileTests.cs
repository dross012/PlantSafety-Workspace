// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Entities
//  Layer:          Tests (Unit)
//  File:           TaskComplexityProfileTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Invariant tests for the TaskComplexityProfile child entity constructor.
//                   Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Entities;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Entities;

public sealed class TaskComplexityProfileTests : UnitTestBase
{
    private static TaskComplexityProfile Build(
        string taskCode = "CTRL-ROOM-STARTUP", string taskName = "Control Room Startup", int complexity = 7, string description = "desc") =>
        new(TaskComplexityProfileId.New(), taskCode, taskName, complexity, description, FixedTimestamp, TestUserId);

    [Fact]
    public void Constructor_WithValidInputs_SetsAllProperties()
    {
        var profile = Build();

        profile.TaskCode.Should().Be("CTRL-ROOM-STARTUP");
        profile.TaskName.Should().Be("Control Room Startup");
        profile.ComplexityScore.Should().Be(7);
        profile.Description.Should().Be("desc");
    }

    [Fact]
    public void Constructor_TrimsTaskCodeAndTaskNameAndDescription()
    {
        var profile = Build(taskCode: "  X-1  ", taskName: "  Task X  ", description: "  d  ");

        profile.TaskCode.Should().Be("X-1");
        profile.TaskName.Should().Be("Task X");
        profile.Description.Should().Be("d");
    }

    [Fact]
    public void Constructor_WithNullDescription_DefaultsToEmptyString()
    {
        var profile = new TaskComplexityProfile(TaskComplexityProfileId.New(), "X", "Task X", 5, null!, FixedTimestamp, TestUserId);

        profile.Description.Should().Be(string.Empty);
    }

    [Fact]
    public void Constructor_WithBlankTaskCode_ThrowsArgumentException()
    {
        var act = () => Build(taskCode: "   ");

        act.Should().Throw<ArgumentException>().WithParameterName("taskCode");
    }

    [Fact]
    public void Constructor_WithBlankTaskName_ThrowsArgumentException()
    {
        var act = () => Build(taskName: "   ");

        act.Should().Throw<ArgumentException>().WithParameterName("taskName");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Constructor_WithComplexityScoreOutOfRange_ThrowsArgumentOutOfRangeException(int complexity)
    {
        var act = () => Build(complexity: complexity);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("complexityScore");
    }
}
