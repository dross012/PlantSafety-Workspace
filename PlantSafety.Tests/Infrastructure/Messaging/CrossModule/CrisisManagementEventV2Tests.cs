// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Infrastructure.Messaging.CrossModule
//  Layer:          Tests (Unit)
//  File:           CrisisManagementEventV2Tests.cs
//  Created:        2026-08-08
//  Description:    Phase B.3 tests for CrossModuleDomainEventMapperV2's
//                  CrisisManagement coverage -- the first cross-module
//                  contract CrisisManagement has ever had (see
//                  CrisisManagementDeclaredIntegrationEventV2.cs's header).
//                  Isolated, deterministic, no I/O.
// ============================================================================
using CrisisManagementAggregate = PlantSafety.Domain.SafetyEvents.CrisisManagement.Aggregates.CrisisManagement;
using PlantSafety.Domain.SafetyEvents.CrisisManagement.Events;
using PlantSafety.Infrastructure.Messaging.CrossModule;
using PlantSafety.Tests.Common.Extensions;

using Shared.DomainEvents.Abstractions.PlantSafety.CrisisManagement;
using Shared.EventBus.Abstractions.Contracts;

namespace PlantSafety.Tests.Infrastructure.Messaging.CrossModule;

/// <summary>Unit tests for <see cref="CrossModuleDomainEventMapperV2"/>'s CrisisManagement coverage.</summary>
public sealed class CrisisManagementEventV2Tests : UnitTestBase
{
    private const string TenantIdString = "00000001-0000-0000-0000-000000000001";

    private static CrisisManagementAggregate DeclareCrisis(string title = "Chemical release, Building 4") =>
        CrisisManagementAggregate.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            siteId: Guid.NewGuid(),
            title: title,
            severity: "Critical",
            declaredAtUtc: FixedTimestamp);

    [Fact]
    public void TryMap_CrisisManagementDeclaredEvent_MapsToV2()
    {
        CrisisManagementAggregate agg = DeclareCrisis();
        var domainEvent = agg.DomainEvents.OfType<CrisisManagementDeclaredEvent>().Single();

        bool mapped = CrossModuleDomainEventMapperV2.TryMap(domainEvent, TenantIdString, out EventEnvelopeV2? envelope);

        mapped.Should().BeTrue();
        envelope.Should().NotBeNull();
        envelope!.EventType.Should().Be(CrisisManagementDeclaredIntegrationEventV2.EventType);
        envelope.PayloadType.Should().Be(nameof(CrisisManagementDeclaredIntegrationEventV2));
        envelope.SchemaVersion.Should().Be(2);

        CrisisManagementDeclaredIntegrationEventV2 payload = envelope.DeserializePayload<CrisisManagementDeclaredIntegrationEventV2>();
        payload.CrisisManagementId.Should().Be(agg.CrisisManagementId.Value);
        payload.Title.Should().Be("Chemical release, Building 4");
        payload.Severity.Should().Be("Critical");
    }

    [Fact]
    public void TryMap_CrisisManagementDeclaredEvent_InfersHazardTypeFromTitle()
    {
        CrisisManagementAggregate agg = DeclareCrisis(title: "Chemical release, Building 4");
        var domainEvent = agg.DomainEvents.OfType<CrisisManagementDeclaredEvent>().Single();

        CrossModuleDomainEventMapperV2.TryMap(domainEvent, TenantIdString, out EventEnvelopeV2? envelope);

        // The real event's Title field ("Chemical release...") is genuine free text this mapper can
        // classify against -- unlike most other V2 events, which have no descriptive text at all.
        envelope!.HazardType.Should().Be("ChemicalSpill");
    }

    [Fact]
    public void TryMap_CrisisManagementDeclaredEvent_NoV1Coverage_ConfirmsFirstEverContract()
    {
        CrisisManagementAggregate agg = DeclareCrisis();
        var domainEvent = agg.DomainEvents.OfType<CrisisManagementDeclaredEvent>().Single();

        bool mappedByV1 = CrossModuleDomainEventMapper.TryMap(domainEvent, TenantIdString, out _);

        mappedByV1.Should().BeFalse("CrisisManagement had no cross-module contract of any kind before Phase B.3");
    }
}
