/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               MessageAcknowledgeResult.cs
    DESCRIPTION:        Strong-typed result for message handling. Determines whether the message is acknowledged, retried,
                        or dead-lettered.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Messaging;

/// <summary>
/// Strong-typed result for message handling.
/// </summary>
public enum MessageAcknowledgeResult
{
    Ack = 0,
    Retry = 1,
    DeadLetter = 2
}
