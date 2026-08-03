// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging
//  Layer:          Infrastructure
//  File:           OutboxEventTypeRegistry.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 §3 Cross-Module Orchestration)
//  SchemaVersion:  2026.1
//  Description:    Resolves a canonical OutboxMessages.EventType discriminator string (e.g.
//                  "RiskManagementCreated") back to the concrete CLR domain-event record type
//                  (e.g. RiskManagementCreatedEvent) so the OutboxProcessor can publish the real,
//                  strongly-typed event to the bus instead of the raw envelope wrapper -- which is
//                  what every existing MassTransit IConsumer&lt;TConcreteEvent&gt; (e.g.
//                  RiskManagementCreatedConsumer : IConsumer&lt;RiskManagementCreatedEvent&gt;)
//                  actually expects to receive. Convention (observed across every existing domain
//                  event in PlantSafety.Domain): EventType == typeof(TEvent).Name with the
//                  trailing "Event" suffix removed. Additive-only evolution.
// ============================================================================
using System.Collections.Concurrent;
using System.Reflection;
using PlantSafety.Domain.Common.Abstractions;

namespace PlantSafety.Infrastructure.Messaging;

/// <summary>
/// Reflection-built, lazily-initialised map from canonical outbox <c>EventType</c> discriminator
/// strings to the concrete <see cref="IDomainEvent"/> CLR type that produced them.
/// </summary>
public static class OutboxEventTypeRegistry
{
    private static readonly Lazy<IReadOnlyDictionary<string, Type>> Map = new(Build);

    /// <summary>
    /// Attempts to resolve <paramref name="eventType"/> (e.g. <c>"RiskManagementCreated"</c>)
    /// to the concrete domain-event CLR type (e.g. <c>RiskManagementCreatedEvent</c>).
    /// </summary>
    public static bool TryResolve(string eventType, out Type? domainEventType) =>
        Map.Value.TryGetValue(eventType, out domainEventType);

    private static IReadOnlyDictionary<string, Type> Build()
    {
        var result = new ConcurrentDictionary<string, Type>(StringComparer.Ordinal);

        // The Domain assembly (identified via the canonical IDomainEvent marker interface) is the
        // single source of truth for every domain event type across every module -- scanning it
        // once here avoids needing a hand-maintained registration per module, matching the
        // "additive-only, zero drift" convention already established for EventType naming.
        var domainAssembly = typeof(IDomainEvent).Assembly;

        foreach (var type in domainAssembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface) continue;
            if (!typeof(IDomainEvent).IsAssignableFrom(type)) continue;

            var discriminator = ResolveDiscriminator(type);
            if (string.IsNullOrWhiteSpace(discriminator)) continue;

            // First registration wins; duplicate discriminators across modules would indicate a
            // genuine naming collision that should be caught by module-level review, not silently
            // overwritten here.
            result.TryAdd(discriminator, type);
        }

        return result;
    }

    private static string? ResolveDiscriminator(Type domainEventType)
    {
        try
        {
            // Every observed domain event exposes `EventType` as a fixed (non-instance-dependent)
            // expression-bodied property (e.g. `public string EventType => "RiskManagementCreated";`).
            // Instantiating via RuntimeHelpers.GetUninitializedObject avoids requiring a
            // parameterless constructor (records here are positional) while still allowing the
            // expression-bodied getter -- which does not touch instance fields -- to execute safely.
            var instance = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(domainEventType);
            var property = domainEventType.GetProperty(nameof(IDomainEvent.EventType), BindingFlags.Public | BindingFlags.Instance);
            return property?.GetValue(instance) as string;
        }
        catch
        {
            // Fall back to the naming convention if the property getter touches uninitialized
            // instance state (defensive; not expected given the fixed-string convention above).
            return domainEventType.Name.EndsWith("Event", StringComparison.Ordinal)
                ? domainEventType.Name[..^"Event".Length]
                : null;
        }
    }
}
