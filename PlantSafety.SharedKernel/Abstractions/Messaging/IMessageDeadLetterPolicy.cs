/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IMessageDeadLetterPolicy.cs
    DESCRIPTION:        Canonical dead-letter policy abstraction. Determines when a message must be moved to the dead-letter
                        queue.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Messaging;

/// <summary>
/// Determines whether a message should be dead-lettered.
/// MUST be deterministic.
/// </summary>
public interface IMessageDeadLetterPolicy
{
    bool ShouldDeadLetter(MessageEnvelope envelope, int attempt);
}
