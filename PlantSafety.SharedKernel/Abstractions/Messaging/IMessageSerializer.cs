/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IMessageSerializer.cs
    DESCRIPTION:        Canonical serializer for messages. MUST be deterministic, SIEM-safe, and stable across versions.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Messaging;

/// <summary>
/// Serializes messages into deterministic, SIEM-safe representations.
/// </summary>
public interface IMessageSerializer
{
    string Serialize(MessageEnvelope envelope);
}
