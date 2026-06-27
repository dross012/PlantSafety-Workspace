/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               RoutingVersionedEvent.cs
    DESCRIPTION:        Domain event raised whenever a new structural version of a routing is created (version bump for
                        diffing, time-travel, and analytics).

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Events.Base;

namespace PlantSafety.SharedKernel.Events.Routing;

/// <summary>
/// Raised when a new structural version of a routing is created.
/// </summary>
public sealed record RoutingVersionedEvent : DomainEventBase
{
    public string RoutingId { get; init; }

    /// <summary>
    /// New version number after the versioning operation.
    /// </summary>
    public int NewVersion { get; init; }

    /// <summary>
    /// Previous version number before the versioning operation.
    /// </summary>
    public int PreviousVersion { get; init; }

    public RoutingVersionedEvent(
        string routingId,
        int previousVersion,
        int newVersion,
        IDomainEventMetadata metadata)
    {
        Name = EventName.Create("Routing.Routing.Versioned");
        Version = new EventVersion(1);
        Metadata = metadata;

        RoutingId = routingId;
        PreviousVersion = previousVersion;
        NewVersion = newVersion;
    }
}
