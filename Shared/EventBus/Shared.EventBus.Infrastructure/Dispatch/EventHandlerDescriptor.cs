// ============================================================================
//  Project:        Shared.EventBus.Infrastructure
//  File:           EventHandlerDescriptor.cs
//  Created:        2026-07-27
//  Description:    Non-generic wrapper around a registered IEventHandler<TPayload>, so the
//                  dispatcher can resolve/invoke handlers at runtime purely from the
//                  envelope's PayloadType string (no reflection-heavy generic resolution
//                  needed per dispatch call).
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.EventBus.Abstractions.Contracts;
using Shared.EventBus.Abstractions.Handlers;

namespace Shared.EventBus.Infrastructure.Dispatch;

/// <summary>Describes one registered <see cref="IEventHandler{TPayload}"/>.</summary>
public sealed class EventHandlerDescriptor
{
    /// <summary>The consumer group (module) this handler belongs to.</summary>
    public required string ConsumerGroup { get; init; }

    /// <summary>The simple CLR type name of the payload this handler accepts (matches <see cref="EventEnvelope.PayloadType"/>).</summary>
    public required string PayloadTypeName { get; init; }

    /// <summary>Invokes the handler, resolving it fresh from the given scoped service provider (handlers may have scoped dependencies).</summary>
    public required Func<IServiceProvider, EventEnvelope, CancellationToken, Task> Invoke { get; init; }

    /// <summary>
    /// Builds a descriptor for <typeparamref name="THandler"/> handling <typeparamref name="TPayload"/>.
    /// <paramref name="consumerGroup"/> must equal the value <typeparamref name="THandler"/>.ConsumerGroup
    /// returns at runtime -- passed explicitly here (rather than instantiated eagerly) so
    /// registration never needs to construct a handler outside its proper DI scope.
    /// </summary>
    public static EventHandlerDescriptor For<TPayload, THandler>(string consumerGroup)
        where TPayload : notnull
        where THandler : class, IEventHandler<TPayload>
    {
        return new EventHandlerDescriptor
        {
            ConsumerGroup = consumerGroup,
            PayloadTypeName = typeof(TPayload).Name,
            Invoke = async (services, envelope, ct) =>
            {
                var handler = services.GetRequiredService<THandler>();
                var payload = envelope.DeserializePayload<TPayload>();
                await handler.HandleAsync(payload, envelope, ct);
            },
        };
    }
}
