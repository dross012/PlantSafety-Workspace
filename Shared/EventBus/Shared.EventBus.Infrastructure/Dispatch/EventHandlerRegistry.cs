// ============================================================================
//  Project:        Shared.EventBus.Infrastructure
//  File:           EventHandlerRegistry.cs
//  Created:        2026-07-27
//  Description:    Process-wide registry of EventHandlerDescriptors, populated at DI
//                  registration time via ServiceCollectionExtensions.AddEventHandler<,>().
// ============================================================================
namespace Shared.EventBus.Infrastructure.Dispatch;

/// <summary>Holds all registered <see cref="EventHandlerDescriptor"/>s, queryable by consumer group.</summary>
public sealed class EventHandlerRegistry
{
    private readonly List<EventHandlerDescriptor> _descriptors = [];

    /// <summary>Registers a descriptor. Called once per handler type at startup.</summary>
    public void Register(EventHandlerDescriptor descriptor) => _descriptors.Add(descriptor);

    /// <summary>All distinct consumer group names with at least one registered handler.</summary>
    public IReadOnlyCollection<string> ConsumerGroups
        => _descriptors.Select(d => d.ConsumerGroup).Distinct(StringComparer.Ordinal).ToArray();

    /// <summary>Finds the descriptor (if any) for the given consumer group + payload type name.</summary>
    public EventHandlerDescriptor? Find(string consumerGroup, string payloadTypeName)
        => _descriptors.FirstOrDefault(d =>
            string.Equals(d.ConsumerGroup, consumerGroup, StringComparison.Ordinal) &&
            string.Equals(d.PayloadTypeName, payloadTypeName, StringComparison.Ordinal));
}
