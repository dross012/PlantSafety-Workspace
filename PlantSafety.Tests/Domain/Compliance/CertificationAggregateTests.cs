// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.Certification
//  Layer:          Tests (Unit)
//  File:           CertificationAggregateTests.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Certification aggregate in the Compliance bounded context.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.Certification.Aggregates;
using PlantSafety.Domain.Compliance.Certification.Events;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.Compliance.Certification;

/// <summary>Unit tests for <see cref="Certification"/> aggregate in <c>Compliance</c>.</summary>
public sealed class CertificationAggregateTests : UnitTestBase
{
    [Fact]
    public void Create_ShouldReturnAggregate() =>
        Certification.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor)
               .Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusCreated()
    {
        var agg = Certification.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.Status.Should().Be(CertificationStatus.Created);
    }

    [Fact]
    public void Create_ShouldRaiseCreatedEvent()
    {
        var agg = Certification.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.DomainEvents.ShouldRaiseSingleEvent<CertificationCreatedEvent>();
    }

    [Fact]
    public void Create_Event_ShouldHaveSchemaVersion2026()
    {
        var agg = Certification.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<CertificationCreatedEvent>();
        evt.SchemaVersion.Should().Be("2026.1");
    }

    [Fact]
    public void Create_ShouldAssignNonEmptyId()
    {
        var agg = Certification.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.CertificationId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_ShouldSetCreatedBy()
    {
        var agg = Certification.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.CreatedBy.Should().Be(TestUserId);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtUtc()
    {
        var agg = Certification.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.CreatedAtUtc.Should().Be(FixedTimestamp);
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyList()
    {
        var agg = Certification.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        agg.ClearDomainEvents();
        agg.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void TwoAggregates_ShouldHaveDifferentIds()
    {
        var a1 = Certification.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var a2 = Certification.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        a1.CertificationId.Should().NotBe(a2.CertificationId);
    }

    [Fact]
    public void Create_Event_OccurredAtUtc_ShouldMatchTimestamp()
    {
        var agg = Certification.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<CertificationCreatedEvent>();
        evt.OccurredAtUtc.Should().Be(FixedTimestamp);
    }
}
