/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               EventMetadata.cs
    DESCRIPTION:        Canonical metadata envelope for domain events. Contains correlation, causation, timestamps, actor
                        identity, module identity, and deterministic lineage fields. MUST follow the Canonical Event
                        Envelope Rule and Canonical Metadata Schema.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Events.Metadata;

/// <summary>
/// Canonical metadata envelope for domain events.
/// </summary>
public sealed record EventMetadata(
    EventIdentity Identity,
    EventLineage Lineage,
    EventModule Module,
    EventActor Actor,
    EventTimestamp Timestamp
);
