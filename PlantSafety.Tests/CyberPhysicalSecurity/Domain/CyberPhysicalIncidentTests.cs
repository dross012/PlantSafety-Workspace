// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Domain
//  Layer:          Tests (Unit)
//  File:           CyberPhysicalIncidentTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Functional/lifecycle unit tests for the CyberPhysicalIncident aggregate root
//                  (Incident Response subdomain). Covers Open, the full lifecycle transition map
//                  (Open->Investigating->Contained->Resolved->Closed and shortcuts), the
//                  ResponseTimeMinutes KPI computation, and invalid transitions.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Events;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Exceptions;
using CyberPhysicalIncidentAggregate = PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Aggregates.CyberPhysicalIncident;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Domain;

public sealed class CyberPhysicalIncidentTests : UnitTestBase
{
    private static CyberPhysicalIncidentAggregate OpenedIncident(
        CyberPhysicalIncidentSeverity severity = CyberPhysicalIncidentSeverity.High,
        IEnumerable<Guid>? relatedIndicatorIds = null) =>
        CyberPhysicalIncidentAggregate.Open(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: Guid.NewGuid(),
            title: "Coordinated PLC setpoint tampering",
            description: "Multi-source correlated sabotage indicators",
            severity: severity,
            ownerId: TestUserId,
            relatedSabotageIndicatorIds: relatedIndicatorIds);

    [Fact]
    public void Open_WithValidData_CreatesOpenIncidentAndRaisesEvent()
    {
        var incident = OpenedIncident();

        incident.Status.Should().Be(CyberPhysicalIncidentStatus.Open);
        incident.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<CyberPhysicalIncidentOpenedEvent>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Open_WithMissingTitle_ThrowsDomainException(string? title)
    {
        var act = () => CyberPhysicalIncidentAggregate.Open(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
            causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
            title: title!, description: "desc", severity: CyberPhysicalIncidentSeverity.High, ownerId: TestUserId);

        act.Should().Throw<CyberPhysicalIncidentDomainException>();
    }

    [Fact]
    public void Open_WithRelatedSabotageIndicatorIds_PopulatesList()
    {
        var relatedIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        var incident = OpenedIncident(relatedIndicatorIds: relatedIds);

        incident.RelatedSabotageIndicatorIds.Should().BeEquivalentTo(relatedIds);
    }

    [Fact]
    public void BeginInvestigation_FromOpen_TransitionsToInvestigating()
    {
        var incident = OpenedIncident();

        incident.BeginInvestigation();

        incident.Status.Should().Be(CyberPhysicalIncidentStatus.Investigating);
    }

    [Fact]
    public void Contain_FromInvestigating_TransitionsToContained()
    {
        var incident = OpenedIncident();
        incident.BeginInvestigation();

        incident.Contain();

        incident.Status.Should().Be(CyberPhysicalIncidentStatus.Contained);
    }

    [Fact]
    public void Contain_FromOpen_ThrowsInvalidTransition()
    {
        var incident = OpenedIncident();

        var act = incident.Contain;

        act.Should().Throw<CyberPhysicalIncidentInvalidStatusTransitionException>();
    }

    [Fact]
    public void Resolve_FromContained_TransitionsToResolved()
    {
        var incident = OpenedIncident();
        incident.BeginInvestigation();
        incident.Contain();

        incident.Resolve();

        incident.Status.Should().Be(CyberPhysicalIncidentStatus.Resolved);
    }

    [Fact]
    public void Close_DirectlyFromOpen_IsAllowedByLenientTransitionMap()
    {
        var incident = OpenedIncident();
        incident.ClearDomainEvents();
        var closedAt = UtcTimestamp.From(FixedUtcNow.AddMinutes(45));

        incident.Close(closedAt, TestActor, TestCorrelationId, TestCausationId);

        incident.Status.Should().Be(CyberPhysicalIncidentStatus.Closed);
        incident.ClosedAtUtc.Should().Be(closedAt);
        incident.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<CyberPhysicalIncidentClosedEvent>();
    }

    [Fact]
    public void Close_ComputesResponseTimeMinutesFromOpenedToClosedAt()
    {
        var incident = OpenedIncident();
        var closedAt = UtcTimestamp.From(FixedUtcNow.AddMinutes(137));

        incident.Close(closedAt, TestActor, TestCorrelationId, TestCausationId);

        incident.ResponseTimeMinutes.Should().Be(137);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ThrowsInvalidTransition()
    {
        var incident = OpenedIncident();
        incident.Close(UtcTimestamp.From(FixedUtcNow.AddMinutes(10)), TestActor, TestCorrelationId, TestCausationId);

        var act = () => incident.Close(UtcTimestamp.From(FixedUtcNow.AddMinutes(20)), TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<CyberPhysicalIncidentInvalidStatusTransitionException>();
    }

    [Fact]
    public void FullLifecycle_OpenToInvestigatingToContainedToResolvedToClosed_Succeeds()
    {
        var incident = OpenedIncident();

        incident.BeginInvestigation();
        incident.Contain();
        incident.Resolve();
        incident.Close(UtcTimestamp.From(FixedUtcNow.AddHours(2)), TestActor, TestCorrelationId, TestCausationId);

        incident.Status.Should().Be(CyberPhysicalIncidentStatus.Closed);
        incident.ResponseTimeMinutes.Should().Be(120);
    }
}
