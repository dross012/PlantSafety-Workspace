// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Compliance
//  Layer:          Tests (Unit)
//  File:           ComplianceAggregateTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Compliance aggregate in the Compliance bounded context.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.Compliance.Aggregates;
using PlantSafety.Domain.Compliance.Compliance.Events;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.Compliance.Compliance;

/// <summary>Unit tests for <see cref="Compliance"/> aggregate in <c>Compliance</c>.</summary>
public sealed class ComplianceAggregateTests : UnitTestBase
{
    [Fact]
    public void Create_ShouldReturnAggregate() =>
        Compliance.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor)
               .Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusCreated()
    {
        var agg = Compliance.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.Status.Should().Be(ComplianceStatus.Created);
    }

    [Fact]
    public void Create_ShouldRaiseCreatedEvent()
    {
        var agg = Compliance.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.DomainEvents.ShouldRaiseSingleEvent<ComplianceCreatedEvent>();
    }

    [Fact]
    public void Create_Event_ShouldHaveSchemaVersion2026()
    {
        var agg = Compliance.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<ComplianceCreatedEvent>();
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void Create_ShouldAssignNonEmptyId()
    {
        var agg = Compliance.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.ComplianceId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldSetCreatedBy()
    {
        var agg = Compliance.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.CreatedBy.Should().Be(TestUserId);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtUtc()
    {
        var agg = Compliance.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.CreatedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyList()
    {
        var agg = Compliance.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.ClearDomainEvents();
        agg.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = Compliance.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var a2 = Compliance.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        a1.ComplianceId.Should().NotBe(a2.ComplianceId);
    }

    [Fact]
    public void Create_Event_OccurredAtUtc_ShouldMatchTimestamp()
    {
        var agg = Compliance.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<ComplianceCreatedEvent>();
        evt.OccurredAtUtc.Should().Be(FixedTimestamp);
    }
}
