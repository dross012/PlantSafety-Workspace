// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Compliance
//  Layer:          Tests (Unit)
//  File:           AuditingAggregateTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Auditing aggregate root in the Compliance bounded context.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.Auditing.Aggregates;
using PlantSafety.Domain.Compliance.Auditing.Events;
using PlantSafety.Tests.Common.Extensions;
using PlantSafety.Domain.Compliance.Auditing.Enums;

namespace PlantSafety.Tests.Domain.Compliance;

/// <summary>Unit tests for <see cref="Auditing"/> aggregate in the Compliance bounded context.</summary>
public sealed class AuditingAggregateTests : UnitTestBase
{
    [Fact]
    public void Create_ShouldReturnAggregate() =>
        Auditing.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor)
                .Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusCreated()
    {
        var agg = Auditing.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.Status.Should().Be(AuditingStatus.Created);
    }

    [Fact]
    public void Create_ShouldRaiseAuditingCreatedEvent()
    {
        var agg = Auditing.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.DomainEvents.ShouldRaiseSingleEvent<AuditingCreatedEvent>();
    }

    [Fact]
    public void Create_Event_SchemaVersion_ShouldBe2026()
    {
        var agg = Auditing.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<AuditingCreatedEvent>();
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmpty()
    {
        var agg = Auditing.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.ClearDomainEvents();
        agg.DomainEvents.Should().BeEmpty();
    }
}
