/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               RoutingCreatedEvent.cs
    DESCRIPTION:        Domain event raised when a new routing is created.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Events.Base;

namespace PlantSafety.SharedKernel.Events.Routing;

/// <summary>
/// Raised when a new routing is created.
/// </summary>
public sealed record RoutingCreatedEvent : DomainEventBase
{
    /// <summary>
    /// Canonical routing identifier (StrongId or GUID as string).
    /// </summary>
    public string RoutingId { get; init; }

    /// <summary>
    /// Human-readable routing name.
    /// </summary>
    public string RoutingName { get; init; }

    /// <summary>
    /// Initial routing version (typically 1).
    /// </summary>
    public int RoutingVersion { get; init; }

    public RoutingCreatedEvent(
        string routingId,
        string routingName,
        int routingVersion,
        IDomainEventMetadata metadata)
    {
        Name = EventName.Create("Routing.Routing.Created");
        Version = new EventVersion(1);
        Metadata = metadata;

        RoutingId = routingId;
        RoutingName = routingName;
        RoutingVersion = routingVersion;
    }
}
