// ============================================================================
//  Project:        Shared.EventBus.Tests
//  File:           InMemoryEventPublisherTests.cs
//  Created:        2026-07-27
//  Description:    Event emission tests against InMemoryEventBus (the SqlEventPublisher
//                  requires a live SQL Server instance and is exercised by
//                  PlantSafety.IntegrationTests / DataIntegrationTests instead -- see
//                  docs/architecture/eventbus-architecture.md §"Testing").
// ============================================================================
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Shared.DomainEvents.Abstractions.PlantSafety.IncidentManagement;
using Shared.EventBus.Abstractions.Contracts;
using Shared.EventBus.Infrastructure.Dispatch;
using Shared.EventBus.Infrastructure.InMemory;
using Shared.EventBus.Infrastructure.Options;

using Xunit;

namespace Shared.EventBus.Tests.Publishing;

public sealed class InMemoryEventPublisherTests
{
    private static InMemoryEventBus CreateBus()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        return new InMemoryEventBus(new EventHandlerRegistry(), new EventBusOptions(), NullLoggerFactory.Instance, provider);
    }

    [Fact]
    public async Task PublishAsync_Envelope_IsRecordedInPublishedEvents()
    {
        var bus = CreateBus();
        var payload = new IncidentManagementCreatedIntegrationEvent(
            Guid.NewGuid(), "tenant-1", DateTime.UtcNow, Guid.NewGuid(), "User", Guid.NewGuid(), Guid.NewGuid());

        var envelope = EventEnvelope.Create(
            payload, payload.TenantId, payload.CorrelationId, payload.CausationId, payload.CreatedAtUtc,
            "PlantSafety", IncidentManagementCreatedIntegrationEvent.EventType, IncidentManagementCreatedIntegrationEvent.SchemaVersion);

        await bus.PublishAsync(envelope);

        Assert.Single(bus.PublishedEvents);
        Assert.Equal(envelope.EventId, bus.PublishedEvents.Single().EventId);
    }

    [Fact]
    public async Task PublishAsync_TypedOverload_BuildsEnvelopeWithCanonicalEventType()
    {
        var bus = CreateBus();
        var payload = new IncidentManagementCreatedIntegrationEvent(
            Guid.NewGuid(), "tenant-1", DateTime.UtcNow, null, "System", Guid.NewGuid(), Guid.NewGuid());

        await bus.PublishAsync(
            payload, payload.TenantId, payload.CorrelationId, payload.CausationId, payload.CreatedAtUtc,
            "PlantSafety", IncidentManagementCreatedIntegrationEvent.EventType, IncidentManagementCreatedIntegrationEvent.SchemaVersion);

        var published = Assert.Single(bus.PublishedEvents);
        Assert.Equal("Incidents.IncidentManagementCreated", published.EventType);
        Assert.Equal("PlantSafety", published.SourceModule);
        Assert.Equal("tenant-1", published.TenantId);
    }

    [Fact]
    public async Task PublishAsync_MultipleEvents_PreservesPublishOrder()
    {
        var bus = CreateBus();
        for (var i = 0; i < 5; i++)
        {
            var payload = new IncidentManagementCreatedIntegrationEvent(
                Guid.NewGuid(), "tenant-1", DateTime.UtcNow, null, "System", Guid.NewGuid(), Guid.NewGuid());
            await bus.PublishAsync(
                payload, payload.TenantId, payload.CorrelationId, payload.CausationId, payload.CreatedAtUtc,
                "PlantSafety", IncidentManagementCreatedIntegrationEvent.EventType, IncidentManagementCreatedIntegrationEvent.SchemaVersion,
                CancellationToken.None);
        }

        Assert.Equal(5, bus.PublishedEvents.Count);
    }
}
