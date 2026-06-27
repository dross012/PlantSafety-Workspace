/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IMessageMetadata.cs
    DESCRIPTION:        Strong-typed metadata envelope for all messages. Includes correlation, causation, timestamps,
                        actor identity, module identity, and deterministic lineage fields.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Abstractions.Messaging;

/// <summary>
/// Strong-typed metadata envelope for all messages.
/// MUST follow Canonical Event Envelope Schema.
/// </summary>
public interface IMessageMetadata
{
    string MessageId { get; }
    string CorrelationId { get; }
    string? CausationId { get; }
    string Module { get; }
    string Actor { get; }
    DateTime UtcTimestamp { get; }
}
