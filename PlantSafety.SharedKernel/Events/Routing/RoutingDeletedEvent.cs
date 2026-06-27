/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               RoutingDeletedEvent.cs
    DESCRIPTION:        Domain event raised when a routing is deleted (soft or hard, per domain rules).

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Events.Base;

namespace PlantSafety.SharedKernel.Events.Routing;

/// <summary>
/// Raised when a routing is deleted.
/// </summary>
public sealed record RoutingDeletedEvent : DomainEventBase
{
    public string RoutingId { get; init; }
    public int RoutingVersion { get; init; }

    public RoutingDeletedEvent(
        string routingId,
        int routingVersion,
        IDomainEventMetadata metadata)
    {
        Name = EventName.Create("Routing.Routing.Deleted");
        Version = new EventVersion(1);
        Metadata = metadata;

        RoutingId = routingId;
        RoutingVersion = routingVersion;
    }
}
