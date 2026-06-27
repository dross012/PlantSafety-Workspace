// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.OperationalSafety.RiskManagement
//  Layer:          Tests (Unit)
//  File:           RiskManagementAggregateTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the RiskManagement aggregate in the OperationalSafety bounded context.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Operational.RiskManagement.Aggregates;
using PlantSafety.Domain.Operational.RiskManagement.Events;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.OperationalSafety.RiskManagement;

/// <summary>Unit tests for <see cref="RiskManagement"/> aggregate in <c>OperationalSafety</c>.</summary>
public sealed class RiskManagementAggregateTests : UnitTestBase
{
    [Fact]
    public void Create_ShouldReturnAggregate() =>
        RiskManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor)
               .Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusCreated()
    {
        var agg = RiskManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.Status.Should().Be(RiskManagementStatus.Created);
    }

    [Fact]
    public void Create_ShouldRaiseCreatedEvent()
    {
        var agg = RiskManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.DomainEvents.ShouldRaiseSingleEvent<RiskManagementCreatedEvent>();
    }

    [Fact]
    public void Create_Event_ShouldHaveSchemaVersion2026()
    {
        var agg = RiskManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<RiskManagementCreatedEvent>();
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void Create_ShouldAssignNonEmptyId()
    {
        var agg = RiskManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.RiskManagementId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldSetCreatedBy()
    {
        var agg = RiskManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.CreatedBy.Should().Be(TestUserId);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtUtc()
    {
        var agg = RiskManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.CreatedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyList()
    {
        var agg = RiskManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.ClearDomainEvents();
        agg.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = RiskManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var a2 = RiskManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        a1.RiskManagementId.Should().NotBe(a2.RiskManagementId);
    }

    [Fact]
    public void Create_Event_OccurredAtUtc_ShouldMatchTimestamp()
    {
        var agg = RiskManagement.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<RiskManagementCreatedEvent>();
        evt.OccurredAtUtc.Should().Be(FixedTimestamp);
    }
}
