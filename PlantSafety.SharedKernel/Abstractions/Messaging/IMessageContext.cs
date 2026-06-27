/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IMessageContext.cs
    DESCRIPTION:        Execution context for message handling. Provides access to metadata, correlation, timestamps, and
                        actor identity.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Messaging;

/// <summary>
/// Execution context for message handling.
/// </summary>
public interface IMessageContext
{
    IMessageMetadata Metadata { get; }
}
