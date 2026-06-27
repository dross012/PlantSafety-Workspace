/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               RoutingPublishedEvent.cs
    DESCRIPTION:        Domain event raised when a routing is published and becomes active/usable.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Events.Base;

namespace PlantSafety.SharedKernel.Events.Routing;

/// <summary>
/// Raised when a routing is published.
/// </summary>
public sealed record RoutingPublishedEvent : DomainEventBase
{
    public string RoutingId { get; init; }
    public int RoutingVersion { get; init; }

    public RoutingPublishedEvent(
        string routingId,
        int routingVersion,
        IDomainEventMetadata metadata)
    {
        Name = EventName.Create("Routing.Routing.Published");
        Version = new EventVersion(1);
        Metadata = metadata;

        RoutingId = routingId;
        RoutingVersion = routingVersion;
    }
}
