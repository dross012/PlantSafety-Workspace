/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               RoutingUpdatedEvent.cs
    DESCRIPTION:        Domain event raised when an existing routing is updated.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Events.Base;

namespace PlantSafety.SharedKernel.Events.Routing;

/// <summary>
/// Raised when an existing routing is updated.
/// </summary>
public sealed record RoutingUpdatedEvent : DomainEventBase
{
    public string RoutingId { get; init; }
    public string RoutingName { get; init; }
    public int RoutingVersion { get; init; }

    public RoutingUpdatedEvent(
        string routingId,
        string routingName,
        int routingVersion,
        IDomainEventMetadata metadata)
    {
        Name = EventName.Create("Routing.Routing.Updated");
        Version = new EventVersion(1);
        Metadata = metadata;

        RoutingId = routingId;
        RoutingName = routingName;
        RoutingVersion = routingVersion;
    }
}
