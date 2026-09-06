// ============================================================================
//  Project:        Shared.EventBus.Infrastructure
//  File:           ServiceCollectionExtensions.cs
//  Created:        2026-07-27
//  Description:    DI wiring for the SQL-backed Shared.EventBus transport, plus the
//                  AddEventHandler<TPayload,THandler> extension consuming modules use to
//                  register their own idempotent handlers.
// ============================================================================
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shared.EventBus.Abstractions.Contracts;
using Shared.EventBus.Abstractions.Handlers;
using Shared.EventBus.Infrastructure.Dispatch;
using Shared.EventBus.Infrastructure.InMemory;
using Shared.EventBus.Infrastructure.Options;
using Shared.EventBus.Infrastructure.Persistence;

namespace Shared.EventBus.Infrastructure.DependencyInjection;

/// <summary>Registration extensions for Shared.EventBus.Infrastructure.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SQL-queue-backed event bus: <see cref="IEventPublisher"/> (SqlEventPublisher),
    /// <see cref="IEventSubscriberFactory"/> (SqlEventSubscriberFactory), and the shared
    /// <see cref="EventHandlerRegistry"/>. Binds <see cref="EventBusOptions"/> from the
    /// "EventBus" configuration section.
    /// </summary>
    public static IServiceCollection AddSharedEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EventBusOptions>(configuration.GetSection(EventBusOptions.SectionName));
        services.TryAddSingleton<EventHandlerRegistry>();
        services.TryAddSingleton<IEventPublisher, SqlEventPublisher>();
        services.TryAddSingleton<IEventSubscriberFactory, SqlEventSubscriberFactory>();
        // Phase B.4: EventEnvelopeV2 publishing over the same dbo.EventQueue table (extended
        // with nullable enrichment columns -- see sql/Phase1-EventEnvelopeV2/). No V2
        // subscriber factory yet; see SqlEventPublisherV2.cs's header.
        services.TryAddSingleton<IEventPublisherV2, SqlEventPublisherV2>();
        return services;
    }

    /// <summary>
    /// Registers the in-memory event bus (tests / local dev without SQL Server). Registers
    /// <see cref="InMemoryEventBus"/> itself (concrete type, for test inspection of
    /// PublishedEvents/DeadLetters) as well as <see cref="IEventPublisher"/>.
    /// </summary>
    public static IServiceCollection AddInMemoryEventBus(this IServiceCollection services, Action<EventBusOptions>? configure = null)
    {
        services.TryAddSingleton<EventHandlerRegistry>();
        services.Configure<EventBusOptions>(o => configure?.Invoke(o));
        services.TryAddSingleton<InMemoryEventBus>(sp => new InMemoryEventBus(
            sp.GetRequiredService<EventHandlerRegistry>(),
            sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<EventBusOptions>>().Value,
            sp.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>(),
            sp));
        services.TryAddSingleton<IEventPublisher>(sp => sp.GetRequiredService<InMemoryEventBus>());
        // Phase B.4: in-memory EventEnvelopeV2 publishing (tests / local dev).
        services.TryAddSingleton<EventBusPublisherV2>();
        services.TryAddSingleton<IEventPublisherV2>(sp => sp.GetRequiredService<EventBusPublisherV2>());
        return services;
    }

    /// <summary>
    /// Registers <typeparamref name="THandler"/> (scoped) as the handler for
    /// <typeparamref name="TPayload"/> within <paramref name="consumerGroup"/>, and records
    /// its descriptor in the shared <see cref="EventHandlerRegistry"/> so subscribers can
    /// dispatch to it purely from the envelope's PayloadType string.
    /// </summary>
    public static IServiceCollection AddEventHandler<TPayload, THandler>(this IServiceCollection services, string consumerGroup)
        where TPayload : notnull
        where THandler : class, IEventHandler<TPayload>
    {
        services.AddScoped<THandler>();

        // EventHandlerRegistry must be populated at *registration* time (before the container
        // is built), not resolution time, so subscribers can dispatch immediately once the
        // provider is built -- no consumer needs to eagerly resolve a "marker" service for
        // registration side effects to take hold. Reuse the same registry instance across
        // repeated AddEventHandler<,> calls by finding any already-registered instance
        // descriptor for EventHandlerRegistry in this IServiceCollection.
        var existing = services.FirstOrDefault(d =>
            d.ServiceType == typeof(EventHandlerRegistry) && d.ImplementationInstance is not null);

        EventHandlerRegistry registry;
        if (existing is not null)
        {
            registry = (EventHandlerRegistry)existing.ImplementationInstance!;
        }
        else
        {
            registry = new EventHandlerRegistry();
            services.AddSingleton(registry);
        }

        registry.Register(EventHandlerDescriptor.For<TPayload, THandler>(consumerGroup));

        return services;
    }
}
