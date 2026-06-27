// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Permits
//  Layer:          Tests (Unit)
//  File:           PermitsAggregateTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Permits aggregate in the Compliance bounded context.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.Permits.Aggregates;
using PlantSafety.Domain.Compliance.Permits.Events;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.Compliance.Permits;

/// <summary>Unit tests for <see cref="Permits"/> aggregate in <c>Compliance</c>.</summary>
public sealed class PermitsAggregateTests : UnitTestBase
{
    [Fact]
    public void Create_ShouldReturnAggregate() =>
        Permits.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor)
               .Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusCreated()
    {
        var agg = Permits.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.Status.Should().Be(PermitsStatus.Created);
    }

    [Fact]
    public void Create_ShouldRaiseCreatedEvent()
    {
        var agg = Permits.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.DomainEvents.ShouldRaiseSingleEvent<PermitsCreatedEvent>();
    }

    [Fact]
    public void Create_Event_ShouldHaveSchemaVersion2026()
    {
        var agg = Permits.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<PermitsCreatedEvent>();
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void Create_ShouldAssignNonEmptyId()
    {
        var agg = Permits.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.PermitsId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldSetCreatedBy()
    {
        var agg = Permits.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.CreatedBy.Should().Be(TestUserId);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtUtc()
    {
        var agg = Permits.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.CreatedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyList()
    {
        var agg = Permits.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.ClearDomainEvents();
        agg.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = Permits.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var a2 = Permits.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        a1.PermitsId.Should().NotBe(a2.PermitsId);
    }

    [Fact]
    public void Create_Event_OccurredAtUtc_ShouldMatchTimestamp()
    {
        var agg = Permits.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<PermitsCreatedEvent>();
        evt.OccurredAtUtc.Should().Be(FixedTimestamp);
    }
}
