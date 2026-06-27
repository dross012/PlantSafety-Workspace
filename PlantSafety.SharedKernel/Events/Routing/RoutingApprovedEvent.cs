/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               RoutingApprovedEvent.cs
    DESCRIPTION:        Domain event raised when a routing is approved in the workflow.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Events.Base;

namespace PlantSafety.SharedKernel.Events.Routing;

/// <summary>
/// Raised when a routing is approved.
/// </summary>
public sealed record RoutingApprovedEvent : DomainEventBase
{
    public string RoutingId { get; init; }
    public int RoutingVersion { get; init; }

    public RoutingApprovedEvent(
        string routingId,
        int routingVersion,
        IDomainEventMetadata metadata)
    {
        Name = EventName.Create("Routing.Routing.Approved");
        Version = new EventVersion(1);
        Metadata = metadata;

        RoutingId = routingId;
        RoutingVersion = routingVersion;
    }
}
