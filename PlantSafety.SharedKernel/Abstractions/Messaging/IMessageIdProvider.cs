/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IMessageIdProvider.cs
    DESCRIPTION:        Provides deterministic UUIDv7 identifiers for messages. Enforces the Deterministic Identifier Rule.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Messaging;

/// <summary>
/// Provides deterministic UUIDv7 identifiers for messages.
/// </summary>
public interface IMessageIdProvider
{
    string CreateMessageId();
}
