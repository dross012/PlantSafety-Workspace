/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               MessageEnvelope.cs
    DESCRIPTION:        Immutable canonical message envelope containing the message payload and metadata. Required by the
                        Canonical Event Envelope Rule and Canonical Messaging Architecture.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Messaging;

/// <summary>
/// Immutable canonical message envelope.
/// </summary>
public sealed record MessageEnvelope(
    IMessage Message,
    IMessageMetadata Metadata);
