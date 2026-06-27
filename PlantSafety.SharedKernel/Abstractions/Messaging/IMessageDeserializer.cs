/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IMessageDeserializer.cs
    DESCRIPTION:        Canonical deserializer for messages. MUST be deterministic, SIEM-safe, and version-aware.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Messaging;

/// <summary>
/// Deserializes messages from deterministic serialized representations.
/// </summary>
public interface IMessageDeserializer
{
    MessageEnvelope Deserialize(string serialized);
}
