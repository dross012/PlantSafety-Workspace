/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               RoutingSubmittedForApprovalEvent.cs
    DESCRIPTION:        Domain event raised when a routing is submitted into an approval workflow.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Events.Base;

namespace PlantSafety.SharedKernel.Events.Routing;

/// <summary>
/// Raised when a routing is submitted for approval.
/// </summary>
public sealed record RoutingSubmittedForApprovalEvent : DomainEventBase
{
    public string RoutingId { get; init; }
    public int RoutingVersion { get; init; }

    public RoutingSubmittedForApprovalEvent(
        string routingId,
        int routingVersion,
        IDomainEventMetadata metadata)
    {
        Name = EventName.Create("Routing.Routing.SubmittedForApproval");
        Version = new EventVersion(1);
        Metadata = metadata;

        RoutingId = routingId;
        RoutingVersion = routingVersion;
    }
}
