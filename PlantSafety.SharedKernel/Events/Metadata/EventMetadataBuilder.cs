/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               EventMetadataBuilder.cs
    DESCRIPTION:        Deterministic builder for constructing canonical event metadata. Enforces UTC timestamps, UUIDv7
                        identifiers, correlation/causation propagation, and SIEM-safe normalization.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Events.Metadata;

/// <summary>
/// Deterministic builder for constructing canonical event metadata.
/// </summary>
public static class EventMetadataBuilder
{
    public static EventMetadata Create(
        string module,
        string actor,
        string correlationId,
        string? causationId = null)
    {
        return new EventMetadata(
            Identity: EventIdentity.Create(),
            Lineage: EventLineage.Create(correlationId, causationId),
            Module: EventModule.Create(module),
            Actor: EventActor.Create(actor),
            Timestamp: EventTimestamp.UtcNow()
        );
    }
}
