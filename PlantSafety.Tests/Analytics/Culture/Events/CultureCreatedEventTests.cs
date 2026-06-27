// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.Culture.Events
//  Layer:          Tests (Unit)
//  File:           CultureCreatedEventTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for CultureCreatedEvent schema compliance.
// ============================================================================
using PlantSafety.Domain.Analytics.Culture.Events;
using PlantSafety.Domain.Analytics.Culture.ValueObjects;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.Analytics.Culture.Events;

public sealed class CultureCreatedEventTests : UnitTestBase
{
    private static CultureCreatedEvent BuildEvent() =>
        new(EventId:       new CultureEventId(new Guid("00000010-0000-0000-0000-000000000010")),
            CultureId:      CultureId.New(),
            CreatedAtUtc:  FixedTimestamp,
            Actor:         TestActor,
            CorrelationId: TestCorrelationId,
            CausationId:   TestCausationId);

    [Fact]
    public void SchemaVersion_ShouldBe2026() =>
        BuildEvent().SchemaVersion.Should().Be("2026.1");

    [Fact]
    public void EventVersion_ShouldBeAtLeast1() =>
        BuildEvent().EventVersion.Should().BeGreaterOrEqualTo(1);

    [Fact]
    public void OccurredAtUtc_ShouldMatchInput() =>
        BuildEvent().OccurredAtUtc.Should().Be(FixedTimestamp);

    [Fact]
    public void EventType_ShouldNotBeNullOrEmpty() =>
        BuildEvent().EventType.Should().NotBeNullOrWhiteSpace();
}
