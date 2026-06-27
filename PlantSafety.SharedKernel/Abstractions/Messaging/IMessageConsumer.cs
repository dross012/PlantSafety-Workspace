/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IMessageConsumer.cs
    DESCRIPTION:        Canonical abstraction for consuming messages from the messaging pipeline. Enforces deterministic
                        handling, idempotency, and SIEM-safe behavior.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Messaging;

/// <summary>
/// Consumes messages from the messaging pipeline.
/// MUST be deterministic, idempotent, and SIEM-safe.
/// </summary>
public interface IMessageConsumer
{
    MessageAcknowledgeResult Handle(MessageEnvelope envelope);
}
