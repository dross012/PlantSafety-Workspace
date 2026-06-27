/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               RoutingLockedEvent.cs
    DESCRIPTION:        Domain event raised when a routing is locked for editing or execution safety.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Events.Base;

namespace PlantSafety.SharedKernel.Events.Routing;

/// <summary>
/// Raised when a routing is locked.
/// </summary>
public sealed record RoutingLockedEvent : DomainEventBase
{
    public string RoutingId { get; init; }
    public int RoutingVersion { get; init; }

    public RoutingLockedEvent(
        string routingId,
        int routingVersion,
        IDomainEventMetadata metadata)
    {
        Name = EventName.Create("Routing.Routing.Locked");
        Version = new EventVersion(1);
        Metadata = metadata;

        RoutingId = routingId;
        RoutingVersion = routingVersion;
    }
}
