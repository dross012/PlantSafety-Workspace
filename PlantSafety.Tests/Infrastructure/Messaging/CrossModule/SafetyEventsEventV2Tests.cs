// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Infrastructure.Messaging.CrossModule
//  Layer:          Tests (Unit)
//  File:           SafetyEventsEventV2Tests.cs
//  Created:        2026-08-08
//  Description:    Phase B.3 tests for CrossModuleDomainEventMapperV2's
//                  SafetyEvents (IncidentManagement) coverage. Named
//                  "SafetyEventsEventV2Tests" to match the phase's own
//                  naming, even though the concrete event mapped is
//                  IncidentManagementCreatedEvent -- see
//                  IncidentManagementCreatedIntegrationEventV2.cs's header
//                  for why: no domain event literally named
//                  "SafetyEventOccurred" exists anywhere in PlantSafety.Domain.
//                  Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates;
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;
using PlantSafety.Infrastructure.Messaging.CrossModule;
using PlantSafety.Tests.Common.Extensions;

using Shared.DomainEvents.Abstractions.PlantSafety.IncidentManagement;
using Shared.EventBus.Abstractions.Contracts;

namespace PlantSafety.Tests.Infrastructure.Messaging.CrossModule;

/// <summary>Unit tests for <see cref="CrossModuleDomainEventMapperV2"/>'s SafetyEvents (IncidentManagement) coverage.</summary>
public sealed class SafetyEventsEventV2Tests : UnitTestBase
{
    private const string TenantIdString = "00000001-0000-0000-0000-000000000001";

    private static IncidentManagement CreateIncident() =>
        IncidentManagement.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            siteId: Guid.NewGuid(),
            title: "Near-miss on Line 2",
            description: "Worker nearly struck by moving equipment",
            severity: "Medium",
            location: "Line 2",
            reportedByWorkerId: TestUserId);

    [Fact]
    public void TryMap_IncidentManagementCreatedEvent_MapsToV2WithInferredHazardType()
    {
        IncidentManagement agg = CreateIncident();
        var domainEvent = agg.DomainEvents.OfType<IncidentManagementCreatedEvent>().Single();

        bool mapped = CrossModuleDomainEventMapperV2.TryMap(domainEvent, TenantIdString, out EventEnvelopeV2? envelope);

        mapped.Should().BeTrue();
        envelope.Should().NotBeNull();
        envelope!.EventType.Should().Be(IncidentManagementCreatedIntegrationEventV2.EventType);
        envelope.PayloadType.Should().Be(nameof(IncidentManagementCreatedIntegrationEventV2));
        envelope.SchemaVersion.Should().Be(2);
        envelope.FacilityId.Should().BeNull();
        envelope.HazardType.Should().NotBeNull();

        IncidentManagementCreatedIntegrationEventV2 payload = envelope.DeserializePayload<IncidentManagementCreatedIntegrationEventV2>();
        payload.IncidentManagementId.Should().Be(agg.IncidentManagementId.Value);
    }

    [Fact]
    public void TryMap_IncidentManagementCreatedEvent_V1AlsoStillMaps_LegacyCoexistence()
    {
        IncidentManagement agg = CreateIncident();
        var domainEvent = agg.DomainEvents.OfType<IncidentManagementCreatedEvent>().Single();

        // Phase B rule: "All legacy subscribers must continue consuming EventEnvelope (V1)."
        // Confirm V1's mapper is untouched and still maps this event alongside V2.
        bool mappedByV1 = CrossModuleDomainEventMapper.TryMap(domainEvent, TenantIdString, out _);
        bool mappedByV2 = CrossModuleDomainEventMapperV2.TryMap(domainEvent, TenantIdString, out _);

        mappedByV1.Should().BeTrue();
        mappedByV2.Should().BeTrue();
    }
}
