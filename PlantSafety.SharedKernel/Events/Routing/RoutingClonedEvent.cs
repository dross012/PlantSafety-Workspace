/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               RoutingClonedEvent.cs
    DESCRIPTION:        Domain event raised when a routing is cloned from an existing routing.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Events.Base;

namespace PlantSafety.SharedKernel.Events.Routing;

/// <summary>
/// Raised when a routing is cloned from another routing.
/// </summary>
public sealed record RoutingClonedEvent : DomainEventBase
{
    public string SourceRoutingId { get; init; }
    public string TargetRoutingId { get; init; }
    public int TargetRoutingVersion { get; init; }

    public RoutingClonedEvent(
        string sourceRoutingId,
        string targetRoutingId,
        int targetRoutingVersion,
        IDomainEventMetadata metadata)
    {
        Name = EventName.Create("Routing.Routing.Cloned");
        Version = new EventVersion(1);
        Metadata = metadata;

        SourceRoutingId = sourceRoutingId;
        TargetRoutingId = targetRoutingId;
        TargetRoutingVersion = targetRoutingVersion;
    }
}
