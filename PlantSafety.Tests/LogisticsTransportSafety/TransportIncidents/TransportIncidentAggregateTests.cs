// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.TransportIncidents
//  Layer:          Tests (Unit)
//  File:           TransportIncidentAggregateTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the TransportIncident aggregate root: recording, investigation
//                  start, and closure lifecycle, plus invariant-guard tests for illegal transitions.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates;
using PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Enums;
using PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Events;
using PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Exceptions;
using PlantSafety.Tests.Common.Extensions;

namespace PlantSafety.Tests.LogisticsTransportSafety.TransportIncidents;

/// <summary>Unit tests for <see cref="TransportIncident"/> in the LogisticsTransportSafety bounded context.</summary>
public sealed class TransportIncidentAggregateTests : UnitTestBase
{
    private static readonly Guid VehicleId = new("0000000a-0000-0000-0000-00000000000a");
    private static readonly Guid DriverUserId = new("0000000b-0000-0000-0000-00000000000b");

    private static TransportIncident CreateReported(TransportIncidentSeverity severity = TransportIncidentSeverity.Moderate) =>
        TransportIncident.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            vehicleId: VehicleId,
            driverUserId: DriverUserId,
            incidentType: TransportIncidentType.Collision,
            severity: severity,
            location: "Loading dock 4");

    // ---- Create --------------------------------------------------------------

    [Fact]
    public void Create_ShouldReturnAggregate() =>
        CreateReported().Should().NotBeNull();

    [Fact]
    public void Create_ShouldSetStatusReported()
    {
        var agg = CreateReported();
        agg.Status.Should().Be(TransportIncidentStatus.Reported);
    }

    [Fact]
    public void Create_ShouldTrimLocation()
    {
        var agg = TransportIncident.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, vehicleId: VehicleId, driverUserId: DriverUserId, location: "  Gate 2  ");
        agg.Location.Should().Be("Gate 2");
    }

    [Fact]
    public void Create_ShouldRaiseTransportIncidentRecordedEvent()
    {
        var agg = CreateReported(TransportIncidentSeverity.Critical);
        var evt = agg.DomainEvents.ShouldRaiseSingleEvent<TransportIncidentRecordedEvent>();
        evt.Severity.Should().Be(TransportIncidentSeverity.Critical);
    }

    // ---- BeginInvestigation ------------------------------------------------------

    [Fact]
    public void BeginInvestigation_WhenReported_ShouldTransitionToUnderInvestigation()
    {
        var agg = CreateReported();
        agg.BeginInvestigation(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(TransportIncidentStatus.UnderInvestigation);
    }

    [Fact]
    public void BeginInvestigation_ShouldRaiseEvent()
    {
        var agg = CreateReported();
        agg.ClearDomainEvents();
        agg.BeginInvestigation(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<TransportIncidentInvestigationStartedEvent>();
    }

    [Fact]
    public void BeginInvestigation_WhenAlreadyUnderInvestigation_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateReported();
        agg.BeginInvestigation(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.BeginInvestigation(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<TransportIncidentInvalidStatusTransitionException>();
    }

    // ---- Close --------------------------------------------------------------------

    private static TransportIncident CreateUnderInvestigation()
    {
        var agg = CreateReported();
        agg.BeginInvestigation(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        return agg;
    }

    [Fact]
    public void Close_WhenUnderInvestigation_ShouldTransitionToClosed()
    {
        var agg = CreateUnderInvestigation();
        agg.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Status.Should().Be(TransportIncidentStatus.Closed);
    }

    [Fact]
    public void Close_ShouldRaiseEvent()
    {
        var agg = CreateUnderInvestigation();
        agg.ClearDomainEvents();
        agg.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.DomainEvents.ShouldRaiseSingleEvent<TransportIncidentClosedEvent>();
    }

    [Fact]
    public void Close_WhenReported_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateReported();
        var act = () => agg.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        act.Should().Throw<TransportIncidentInvalidStatusTransitionException>();
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ShouldThrowInvalidStatusTransition()
    {
        var agg = CreateUnderInvestigation();
        agg.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => agg.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<TransportIncidentInvalidStatusTransitionException>();
    }
}
