/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IMessageRetryPolicy.cs
    DESCRIPTION:        Canonical retry policy abstraction. Enforces deterministic, idempotent retry behavior for message
                        handling.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Messaging;

/// <summary>
/// Determines whether a message should be retried.
/// MUST be deterministic and idempotent.
/// </summary>
public interface IMessageRetryPolicy
{
    bool ShouldRetry(MessageEnvelope envelope, int attempt);
}
