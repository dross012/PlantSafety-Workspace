// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SafetyEvents.Observation
//  Layer:          Tests (Unit)
//  File:           ObservationIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ObservationId StrongId value object.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.Observation.ValueObjects.Ids;

namespace PlantSafety.Tests.SafetyEvents.Observation;

public sealed class ObservationIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = ObservationId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => ObservationId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        ObservationId.FromGuid(g).Should().Be(ObservationId.FromGuid(g));
    }

    [Fact]
    public void TwoIds_DifferentInstances_ShouldNotBeEqual() =>
        ObservationId.New().Should().NotBe(ObservationId.New());
}
