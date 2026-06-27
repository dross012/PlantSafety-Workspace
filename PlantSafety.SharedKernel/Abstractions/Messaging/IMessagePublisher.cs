/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IMessagePublisher.cs
    DESCRIPTION:        Canonical abstraction for publishing messages to the messaging pipeline. Enforces deterministic,
                        SIEM-safe, strong-typed publishing behavior.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Messaging;

/// <summary>
/// Publishes messages to the messaging pipeline.
/// MUST be deterministic and idempotent.
/// </summary>
public interface IMessagePublisher
{
    void Publish(MessageEnvelope envelope);
}
