// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Infrastructure.Messaging.CrossModule
//  Layer:          Tests (Unit)
//  File:           AccidentManagementEventV2Tests.cs
//  Created:        2026-08-08
//  Description:    Phase B.3 tests for CrossModuleDomainEventMapperV2's
//                  AccidentManagement case arms. Drives the REAL Accident
//                  aggregate through its full investigation lifecycle
//                  (mirrors AccidentAggregateTests.cs's own sequence) and
//                  maps each raised domain event to its V2 envelope,
//                  asserting EventType/PayloadType/SchemaVersion and that
//                  HazardType is inferred (never null) while FacilityId/
//                  IndustryName/Location stay null pre-enrichment (that step
//                  is CrossModuleEventRelayV2.FlushAsync's job, not the
//                  mapper's -- see CrossModuleDomainEventMapperV2.cs's
//                  header). Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Domain.AccidentManagement.Aggregates;
using PlantSafety.Domain.AccidentManagement.Enums;
using PlantSafety.Domain.AccidentManagement.Events;
using PlantSafety.Domain.Common.Abstractions;
using PlantSafety.Infrastructure.Messaging.CrossModule;
using PlantSafety.Tests.Common.Extensions;

using Shared.DomainEvents.Abstractions.PlantSafety.AccidentManagement;
using Shared.EventBus.Abstractions.Contracts;

namespace PlantSafety.Tests.Infrastructure.Messaging.CrossModule;

/// <summary>Unit tests for <see cref="CrossModuleDomainEventMapperV2"/>'s AccidentManagement coverage.</summary>
public sealed class AccidentManagementEventV2Tests : UnitTestBase
{
    private const string TenantIdString = "00000001-0000-0000-0000-000000000001";

    private static Accident CreateReported() =>
        Accident.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value,
            accidentTitle: "Forklift collision, Dock 3",
            location: "Dock 3",
            severity: AccidentSeverity.LostTimeInjury,
            injuredPersonCount: 1,
            ownerId: TestUserId);

    private static TEvent LatestEventOfType<TEvent>(Accident aggregate) where TEvent : IDomainEvent =>
        aggregate.DomainEvents.OfType<TEvent>().Last();

    [Fact]
    public void TryMap_AccidentCreatedEvent_MapsToV2WithInferredHazardType()
    {
        Accident agg = CreateReported();
        var domainEvent = LatestEventOfType<AccidentCreatedEvent>(agg);

        bool mapped = CrossModuleDomainEventMapperV2.TryMap(domainEvent, TenantIdString, out EventEnvelopeV2? envelope);

        mapped.Should().BeTrue();
        envelope.Should().NotBeNull();
        envelope!.EventType.Should().Be(AccidentCreatedIntegrationEventV2.EventType);
        envelope.PayloadType.Should().Be(nameof(AccidentCreatedIntegrationEventV2));
        envelope.SchemaVersion.Should().Be(2);
        envelope.TenantId.Should().Be(TenantIdString);
        envelope.FacilityId.Should().BeNull();
        envelope.IndustryName.Should().BeNull();
        envelope.Location.Should().BeNull();
        envelope.HazardType.Should().NotBeNull();

        AccidentCreatedIntegrationEventV2 payload = envelope.DeserializePayload<AccidentCreatedIntegrationEventV2>();
        payload.AccidentId.Should().Be(agg.AccidentId.Value);
    }

    [Fact]
    public void TryMap_AccidentArchivedEvent_MapsToV2()
    {
        Accident agg = CreateReported();
        agg.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.FileRegulatoryReport(TestUserId, "OSHA", "REF-001", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CloseInvestigation(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.Archive(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var domainEvent = LatestEventOfType<AccidentArchivedEvent>(agg);
        bool mapped = CrossModuleDomainEventMapperV2.TryMap(domainEvent, TenantIdString, out EventEnvelopeV2? envelope);

        mapped.Should().BeTrue();
        envelope!.EventType.Should().Be(AccidentArchivedIntegrationEventV2.EventType);
    }

    [Fact]
    public void TryMap_AccidentInvestigationClosedEvent_MapsToV2_PreviouslyUncoveredByV1()
    {
        Accident agg = CreateReported();
        agg.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.FileRegulatoryReport(TestUserId, "OSHA", "REF-001", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CloseInvestigation(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var domainEvent = LatestEventOfType<AccidentInvestigationClosedEvent>(agg);

        // Confirm the premise this test guards: V1's mapper has no case arm for this event at all.
        bool mappedByV1 = CrossModuleDomainEventMapper.TryMap(domainEvent, TenantIdString, out _);
        mappedByV1.Should().BeFalse("AccidentInvestigationClosedEvent had no cross-module contract before Phase B.3");

        bool mappedByV2 = CrossModuleDomainEventMapperV2.TryMap(domainEvent, TenantIdString, out EventEnvelopeV2? envelope);
        mappedByV2.Should().BeTrue();
        envelope!.EventType.Should().Be(AccidentInvestigationClosedIntegrationEventV2.EventType);

        AccidentInvestigationClosedIntegrationEventV2 payload = envelope.DeserializePayload<AccidentInvestigationClosedIntegrationEventV2>();
        payload.ClosedBy.Should().Be(TestUserId.Value);
    }

    [Fact]
    public void TryMap_AccidentRootCauseRecordedEvent_MapsToV2WithHazardTypeInferredFromRootCause()
    {
        Accident agg = CreateReported();
        agg.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var domainEvent = LatestEventOfType<AccidentRootCauseRecordedEvent>(agg);
        bool mapped = CrossModuleDomainEventMapperV2.TryMap(domainEvent, TenantIdString, out EventEnvelopeV2? envelope);

        mapped.Should().BeTrue();
        envelope!.EventType.Should().Be(AccidentRootCauseRecordedIntegrationEventV2.EventType);

        AccidentRootCauseRecordedIntegrationEventV2 payload = envelope.DeserializePayload<AccidentRootCauseRecordedIntegrationEventV2>();
        payload.RootCause.Should().Be(RootCauseCategory.Mechanical.ToString());
    }

    [Fact]
    public void TryMap_AccidentCorrectiveActionCreatedEvent_MapsToV2()
    {
        Accident agg = CreateReported();
        agg.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var domainEvent = LatestEventOfType<AccidentCorrectiveActionCreatedEvent>(agg);
        bool mapped = CrossModuleDomainEventMapperV2.TryMap(domainEvent, TenantIdString, out EventEnvelopeV2? envelope);

        mapped.Should().BeTrue();
        envelope!.EventType.Should().Be(AccidentCorrectiveActionCreatedIntegrationEventV2.EventType);

        AccidentCorrectiveActionCreatedIntegrationEventV2 payload = envelope.DeserializePayload<AccidentCorrectiveActionCreatedIntegrationEventV2>();
        payload.Category.Should().Be(CorrectiveActionCategory.EngineeringControls.ToString());
        payload.ResponsiblePersonId.Should().Be(TestUserId.Value);
    }

    [Fact]
    public void TryMap_AccidentRegulatoryFilingCompletedEvent_MapsToV2()
    {
        Accident agg = CreateReported();
        agg.AssignInvestigator(TestUserId, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.RecordRootCause(RootCauseCategory.Mechanical, "Pinch point", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.CreateCorrectiveAction(CorrectiveActionCategory.EngineeringControls, "Install guard", TestUserId, FixedTimestamp, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        agg.FileRegulatoryReport(TestUserId, "OSHA", "REF-001", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var domainEvent = LatestEventOfType<AccidentRegulatoryFilingCompletedEvent>(agg);
        bool mapped = CrossModuleDomainEventMapperV2.TryMap(domainEvent, TenantIdString, out EventEnvelopeV2? envelope);

        mapped.Should().BeTrue();
        envelope!.EventType.Should().Be(AccidentRegulatoryFilingCompletedIntegrationEventV2.EventType);

        AccidentRegulatoryFilingCompletedIntegrationEventV2 payload = envelope.DeserializePayload<AccidentRegulatoryFilingCompletedIntegrationEventV2>();
        payload.RegulatoryAgency.Should().Be("OSHA");
        payload.ReferenceNumber.Should().Be("REF-001");
    }

    [Fact]
    public void TryMap_UnrecognizedDomainEvent_ReturnsFalse()
    {
        var unrelated = new FakeDomainEvent();

        bool mapped = CrossModuleDomainEventMapperV2.TryMap(unrelated, TenantIdString, out EventEnvelopeV2? envelope);

        mapped.Should().BeFalse();
        envelope.Should().BeNull();
    }

    private sealed record FakeDomainEvent : IDomainEvent
    {
        public string EventType => "Fake.Event";
        public int EventVersion => 1;
        public string SchemaVersion => "2026.1";
        public UtcTimestamp OccurredAtUtc => UtcTimestamp.From(DateTime.UtcNow);
    }
}
