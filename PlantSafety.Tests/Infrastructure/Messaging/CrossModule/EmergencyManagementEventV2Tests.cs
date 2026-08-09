// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Infrastructure.Messaging.CrossModule
//  Layer:          Tests (Unit)
//  File:           EmergencyManagementEventV2Tests.cs
//  Created:        2026-08-08
//  Description:    Phase B.3 tests for CrossModuleDomainEventMapperV2's
//                  EmergencyManagement coverage -- the first cross-module
//                  contract EmergencyManagement has ever had (see
//                  EmergencyManagementCreatedIntegrationEventV2.cs's header).
//                  Isolated, deterministic, no I/O.
// ============================================================================
using EmergencyManagementAggregate = PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates.EmergencyManagement;
using PlantSafety.Domain.SafetyEvents.EmergencyManagement.Events;
using PlantSafety.Infrastructure.Messaging.CrossModule;
using PlantSafety.Tests.Common.Extensions;

using Shared.DomainEvents.Abstractions.PlantSafety.EmergencyManagement;
using Shared.EventBus.Abstractions.Contracts;

namespace PlantSafety.Tests.Infrastructure.Messaging.CrossModule;

/// <summary>Unit tests for <see cref="CrossModuleDomainEventMapperV2"/>'s EmergencyManagement coverage.</summary>
public sealed class EmergencyManagementEventV2Tests : UnitTestBase
{
    private const string TenantIdString = "00000001-0000-0000-0000-000000000001";

    private static EmergencyManagementAggregate DeclareEmergency() =>
        EmergencyManagementAggregate.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            siteId: Guid.NewGuid(),
            emergencyType: "Fire",
            severity: "Critical",
            location: "Warehouse B",
            casualtyCount: 0,
            responseTeamNotified: true);

    [Fact]
    public void TryMap_EmergencyManagementCreatedEvent_MapsToV2()
    {
        EmergencyManagementAggregate agg = DeclareEmergency();
        var domainEvent = agg.DomainEvents.OfType<EmergencyManagementCreatedEvent>().Single();

        bool mapped = CrossModuleDomainEventMapperV2.TryMap(domainEvent, TenantIdString, out EventEnvelopeV2? envelope);

        mapped.Should().BeTrue();
        envelope.Should().NotBeNull();
        envelope!.EventType.Should().Be(EmergencyManagementCreatedIntegrationEventV2.EventType);
        envelope.PayloadType.Should().Be(nameof(EmergencyManagementCreatedIntegrationEventV2));
        envelope.SchemaVersion.Should().Be(2);
        envelope.HazardType.Should().NotBeNull();

        EmergencyManagementCreatedIntegrationEventV2 payload = envelope.DeserializePayload<EmergencyManagementCreatedIntegrationEventV2>();
        payload.EmergencyManagementId.Should().Be(agg.EmergencyManagementId.Value);
    }

    [Fact]
    public void TryMap_EmergencyManagementCreatedEvent_NoV1Coverage_ConfirmsFirstEverContract()
    {
        EmergencyManagementAggregate agg = DeclareEmergency();
        var domainEvent = agg.DomainEvents.OfType<EmergencyManagementCreatedEvent>().Single();

        bool mappedByV1 = CrossModuleDomainEventMapper.TryMap(domainEvent, TenantIdString, out _);

        mappedByV1.Should().BeFalse("EmergencyManagement had no cross-module contract of any kind before Phase B.3");
    }
}
