// ============================================================================
//  Project:        Shared.EventBus.Tests
//  File:           EventEnvelopeV2Tests.cs
//  Created:        2026-08-08
//  Description:    Phase B.4 tests: EventEnvelopeV2 construction/round-trip,
//                  EventEnvelopeV1ToV2Adapter's structural upgrade,
//                  FacilityInferenceRules/HazardInferenceRules correctness,
//                  and the in-memory V2 publisher/subscriber pipeline.
// ============================================================================
using FluentAssertions;

using Shared.EventBus.Abstractions.Contracts;
using Shared.EventBus.Infrastructure.Dispatch;
using Shared.EventBus.Infrastructure.InMemory;

using Xunit;

namespace Shared.EventBus.Tests.Contracts;

public sealed record TestPayloadV2(string Value);

public sealed class EventEnvelopeV2Tests
{
    [Fact]
    public void Create_BuildsEnvelopeWithSerializedPayloadAndEnrichment()
    {
        var payload = new TestPayloadV2("hello");
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        EventEnvelopeV2 envelope = EventEnvelopeV2.Create(
            payload, "tenant-1", correlationId, causationId, now, "PlantSafety", "Accidents.AccidentCreatedV2", 2,
            facilityId: facilityId, industryName: "Chemical Manufacturing", location: "Plant A", hazardType: "ChemicalSpill");

        envelope.TenantId.Should().Be("tenant-1");
        envelope.PayloadType.Should().Be(nameof(TestPayloadV2));
        envelope.FacilityId.Should().Be(facilityId);
        envelope.IndustryName.Should().Be("Chemical Manufacturing");
        envelope.Location.Should().Be("Plant A");
        envelope.HazardType.Should().Be("ChemicalSpill");
        envelope.DeserializePayload<TestPayloadV2>().Should().Be(payload);
    }

    [Fact]
    public void Create_NoEnrichmentSupplied_LeavesEnrichmentFieldsNull()
    {
        EventEnvelopeV2 envelope = EventEnvelopeV2.Create(
            new TestPayloadV2("x"), "tenant-1", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, "PlantSafety", "Test.Event", 1);

        envelope.FacilityId.Should().BeNull();
        envelope.IndustryName.Should().BeNull();
        envelope.Location.Should().BeNull();
        envelope.HazardType.Should().BeNull();
    }

    [Fact]
    public void EventEnvelopeV1ToV2Adapter_Upgrade_PreservesFieldsAndNullsEnrichment()
    {
        EventEnvelope v1 = EventEnvelope.Create(
            new TestPayloadV2("legacy"), "tenant-2", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, "PlantSafety", "Legacy.Event", 1);

        EventEnvelopeV2 v2 = EventEnvelopeV1ToV2Adapter.Upgrade(v1);

        v2.EventId.Should().Be(v1.EventId);
        v2.EventType.Should().Be(v1.EventType);
        v2.TenantId.Should().Be(v1.TenantId);
        v2.TimestampUtc.Should().Be(v1.OccurredAtUtc);
        v2.Payload.Should().Be(v1.Payload);
        v2.FacilityId.Should().BeNull();
        v2.IndustryName.Should().BeNull();
        v2.Location.Should().BeNull();
        v2.HazardType.Should().BeNull();
    }

    [Fact]
    public async Task NullFacilityInferenceResolver_AlwaysResolvesUnknown()
    {
        var resolver = new NullFacilityInferenceResolver();

        (Guid? facilityId, string? industryName, string? location) = await resolver.ResolveAsync("any-tenant");

        facilityId.Should().BeNull();
        industryName.Should().BeNull();
        location.Should().BeNull();
    }

    [Fact]
    public async Task FacilityInferenceRules_EnrichWithFacilityAsync_AppliesResolverResult()
    {
        var facilityId = Guid.NewGuid();
        var resolver = new StubFacilityInferenceResolver(facilityId, "Marine Terminals", "Port A");
        EventEnvelopeV2 envelope = EventEnvelopeV2.Create(new TestPayloadV2("x"), "tenant-3", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, "PlantSafety", "Test.Event", 1);

        EventEnvelopeV2 enriched = await FacilityInferenceRules.EnrichWithFacilityAsync(envelope, resolver);

        enriched.FacilityId.Should().Be(facilityId);
        enriched.IndustryName.Should().Be("Marine Terminals");
        enriched.Location.Should().Be("Port A");
    }

    [Theory]
    [InlineData("AccidentManagement.FireEvent", null, "Fire")]
    [InlineData("Accident.Created", "chemical spill in tank farm", "ChemicalSpill")]
    [InlineData("SafetyEvent.Occurred", "worker injury on line 3", "Injury")]
    [InlineData("Crisis.Declared", "environmental release detected", "EnvironmentalRelease")]
    [InlineData("Emergency.Triggered", "confined space entry incident", "ConfinedSpace")]
    [InlineData("Accident.Created", "lockout tagout procedure bypassed", "LOTOFailure")]
    [InlineData("Accident.Created", "hot work permit violation", "HotWork")]
    [InlineData("Accident.Created", "fleet vehicle collision", "FleetIncident")]
    [InlineData("Accident.Created", "unspecified incident", "Other")]
    public void HazardInferenceRules_InferHazardTypeFromAccident_ClassifiesByKeyword(string eventType, string? description, string expected)
    {
        HazardInferenceRules.InferHazardTypeFromAccident(eventType, description).Should().Be(expected);
    }

    [Fact]
    public async Task EventBusPublisherV2_AndSubscriber_DeliverPublishedEnvelope()
    {
        var bus = new EventBusPublisherV2();
        var subscriber = new RecordingSubscriberV2("TestGroup", bus);

        await bus.PublishAsync(new TestPayloadV2("payload-1"), "tenant-4", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, "PlantSafety", "Test.EventV2", 1);

        int processed = await subscriber.ProcessDueEventsAsync();

        processed.Should().Be(1);
        subscriber.HandledEnvelopes.Should().ContainSingle();
    }

    [Fact]
    public async Task EventBusSubscriberV2_ProcessDueEventsAsync_IsIdempotentAcrossCalls()
    {
        var bus = new EventBusPublisherV2();
        var subscriber = new RecordingSubscriberV2("TestGroup", bus);
        await bus.PublishAsync(new TestPayloadV2("payload-1"), "tenant-5", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, "PlantSafety", "Test.EventV2", 1);

        await subscriber.ProcessDueEventsAsync();
        int secondPass = await subscriber.ProcessDueEventsAsync();

        secondPass.Should().Be(0);
        subscriber.HandledEnvelopes.Should().ContainSingle();
    }

    private sealed class StubFacilityInferenceResolver(Guid? facilityId, string? industryName, string? location) : IFacilityInferenceResolver
    {
        public Task<(Guid? FacilityId, string? IndustryName, string? Location)> ResolveAsync(string tenantId, CancellationToken cancellationToken = default) =>
            Task.FromResult((facilityId, industryName, location));
    }

    private sealed class RecordingSubscriberV2(string consumerGroup, EventBusPublisherV2 bus) : EventBusSubscriberV2(consumerGroup, bus)
    {
        public List<EventEnvelopeV2> HandledEnvelopes { get; } = [];

        protected override bool CanHandle(EventEnvelopeV2 envelope) => envelope.PayloadType == nameof(TestPayloadV2);

        protected override Task HandleAsync(EventEnvelopeV2 envelope, CancellationToken cancellationToken)
        {
            HandledEnvelopes.Add(envelope);
            return Task.CompletedTask;
        }
    }
}
