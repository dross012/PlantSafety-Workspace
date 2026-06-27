/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               RoutingRejectedEvent.cs
    DESCRIPTION:        Domain event raised when a routing is rejected in the workflow.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Events.Base;

namespace PlantSafety.SharedKernel.Events.Routing;

/// <summary>
/// Raised when a routing is rejected.
/// </summary>
public sealed record RoutingRejectedEvent : DomainEventBase
{
    public string RoutingId { get; init; }
    public int RoutingVersion { get; init; }
    public string Reason { get; init; }

    public RoutingRejectedEvent(
        string routingId,
        int routingVersion,
        string reason,
        IDomainEventMetadata metadata)
    {
        Name = EventName.Create("Routing.Routing.Rejected");
        Version = new EventVersion(1);
        Metadata = metadata;

        RoutingId = routingId;
        RoutingVersion = routingVersion;
        Reason = reason;
    }
}
