// ============================================================================
//  Project:        Shared.EventBus.Tests
//  File:           InMemoryEventConsumptionTests.cs
//  Created:        2026-07-27
//  Description:    Event consumption tests: idempotent handling, unrelated-payload skip,
//                  and the failure-handling pipeline (retry with backoff bookkeeping,
//                  poison-message detection, dead-letter routing) against InMemoryEventBus.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

using Shared.DomainEvents.Abstractions.PlantSafety.CAPA;
using Shared.DomainEvents.Abstractions.PlantSafety.IncidentManagement;
using Shared.EventBus.Abstractions.Contracts;
using Shared.EventBus.Abstractions.Handlers;
using Shared.EventBus.Infrastructure.Dispatch;
using Shared.EventBus.Infrastructure.InMemory;
using Shared.EventBus.Infrastructure.Options;

using Xunit;

namespace Shared.EventBus.Tests.Dispatch;

public sealed class InMemoryEventConsumptionTests
{
    private sealed class CountingHandler : IEventHandler<IncidentManagementCreatedIntegrationEvent>
    {
        public string ConsumerGroup => "Reporting";
        public int InvocationCount { get; private set; }

        public Task HandleAsync(IncidentManagementCreatedIntegrationEvent payload, EventEnvelope envelope, CancellationToken cancellationToken)
        {
            InvocationCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class AlwaysThrowingHandler : IEventHandler<CorrectiveActionCreatedIntegrationEvent>
    {
        public string ConsumerGroup => "Reporting";
        public int InvocationCount { get; private set; }

        public Task HandleAsync(CorrectiveActionCreatedIntegrationEvent payload, EventEnvelope envelope, CancellationToken cancellationToken)
        {
            InvocationCount++;
            throw new InvalidOperationException("Simulated poison payload.");
        }
    }

    private static (InMemoryEventBus Bus, EventHandlerRegistry Registry, IServiceProvider Provider) CreateBus(EventBusOptions? options = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<CountingHandler>();
        services.AddSingleton<AlwaysThrowingHandler>();
        var provider = services.BuildServiceProvider();

        var registry = new EventHandlerRegistry();
        registry.Register(EventHandlerDescriptor.For<IncidentManagementCreatedIntegrationEvent, CountingHandler>("Reporting"));
        registry.Register(EventHandlerDescriptor.For<CorrectiveActionCreatedIntegrationEvent, AlwaysThrowingHandler>("Reporting"));

        var bus = new InMemoryEventBus(registry, options ?? new EventBusOptions(), NullLoggerFactory.Instance, provider);
        return (bus, registry, provider);
    }

    [Fact]
    public async Task ProcessDueEventsAsync_InvokesRegisteredHandler_ForMatchingPayloadType()
    {
        var (bus, _, provider) = CreateBus();
        var payload = new IncidentManagementCreatedIntegrationEvent(Guid.NewGuid(), "tenant-1", DateTime.UtcNow, null, "System", Guid.NewGuid(), Guid.NewGuid());
        await bus.PublishAsync(payload, "tenant-1", payload.CorrelationId, payload.CausationId, payload.CreatedAtUtc, "PlantSafety",
            IncidentManagementCreatedIntegrationEvent.EventType, IncidentManagementCreatedIntegrationEvent.SchemaVersion);

        var subscriber = bus.CreateSubscriber("Reporting");
        var processed = await subscriber.ProcessDueEventsAsync();

        Assert.Equal(1, processed);
        var handler = provider.GetRequiredService<CountingHandler>();
        Assert.Equal(1, handler.InvocationCount);
    }

    [Fact]
    public async Task ProcessDueEventsAsync_IsIdempotent_SecondPassDoesNotReinvokeHandler()
    {
        var (bus, _, provider) = CreateBus();
        var payload = new IncidentManagementCreatedIntegrationEvent(Guid.NewGuid(), "tenant-1", DateTime.UtcNow, null, "System", Guid.NewGuid(), Guid.NewGuid());
        await bus.PublishAsync(payload, "tenant-1", payload.CorrelationId, payload.CausationId, payload.CreatedAtUtc, "PlantSafety",
            IncidentManagementCreatedIntegrationEvent.EventType, IncidentManagementCreatedIntegrationEvent.SchemaVersion);

        var subscriber = bus.CreateSubscriber("Reporting");
        await subscriber.ProcessDueEventsAsync();
        var secondPassProcessed = await subscriber.ProcessDueEventsAsync();

        Assert.Equal(0, secondPassProcessed);
        var handler = provider.GetRequiredService<CountingHandler>();
        Assert.Equal(1, handler.InvocationCount);
    }

    [Fact]
    public async Task ProcessDueEventsAsync_UnrelatedConsumerGroup_SkipsWithoutInvokingHandler()
    {
        var (bus, _, provider) = CreateBus();
        var payload = new IncidentManagementCreatedIntegrationEvent(Guid.NewGuid(), "tenant-1", DateTime.UtcNow, null, "System", Guid.NewGuid(), Guid.NewGuid());
        await bus.PublishAsync(payload, "tenant-1", payload.CorrelationId, payload.CausationId, payload.CreatedAtUtc, "PlantSafety",
            IncidentManagementCreatedIntegrationEvent.EventType, IncidentManagementCreatedIntegrationEvent.SchemaVersion);

        var subscriber = bus.CreateSubscriber("SomeOtherModuleNotSubscribedToThisPayload");
        var processed = await subscriber.ProcessDueEventsAsync();

        Assert.Equal(1, processed); // marked processed as not-applicable
        var handler = provider.GetRequiredService<CountingHandler>();
        Assert.Equal(0, handler.InvocationCount);
    }

    [Fact]
    public async Task ProcessDueEventsAsync_HandlerThrowsRepeatedly_DeadLettersAsPoisonAfterMaxAttempts()
    {
        var options = new EventBusOptions { MaxDeliveryAttempts = 3, PoisonMessageRepeatThreshold = 2, BackoffBaseDelay = TimeSpan.Zero, BackoffMaxDelay = TimeSpan.Zero };
        var (bus, _, provider) = CreateBus(options);

        var payload = new CorrectiveActionCreatedIntegrationEvent(
            Guid.NewGuid(), "tenant-1", Guid.NewGuid(), "SomeSource", Guid.NewGuid(), DateTime.UtcNow.AddDays(3), DateTime.UtcNow,
            null, "System", Guid.NewGuid(), Guid.NewGuid());
        await bus.PublishAsync(payload, "tenant-1", payload.CorrelationId, payload.CausationId, payload.CreatedAtUtc, "PlantSafety",
            CorrectiveActionCreatedIntegrationEvent.EventType, CorrectiveActionCreatedIntegrationEvent.SchemaVersion);

        var subscriber = bus.CreateSubscriber("Reporting");

        // Zero backoff means each ProcessDueEventsAsync call immediately re-attempts.
        for (var i = 0; i < options.MaxDeliveryAttempts; i++)
        {
            await subscriber.ProcessDueEventsAsync();
        }

        var handler = provider.GetRequiredService<AlwaysThrowingHandler>();
        Assert.Equal(options.MaxDeliveryAttempts, handler.InvocationCount);

        var deadLetter = Assert.Single(bus.DeadLetters);
        Assert.Equal("Reporting", deadLetter.ConsumerGroup);
        Assert.Equal(payload.CorrectiveActionId, deadLetter.Envelope.DeserializePayload<CorrectiveActionCreatedIntegrationEvent>().CorrectiveActionId);
        Assert.Equal(options.MaxDeliveryAttempts, deadLetter.AttemptCount);
        Assert.True(deadLetter.IsPoisonMessage);

        // Once dead-lettered, further passes must not re-invoke the handler again.
        await subscriber.ProcessDueEventsAsync();
        Assert.Equal(options.MaxDeliveryAttempts, handler.InvocationCount);
    }
}
