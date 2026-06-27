/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IMessageTimestampProvider.cs
    DESCRIPTION:        Provides deterministic UTC timestamps for messages. Enforces the Deterministic Time Rule.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Abstractions.Messaging;

/// <summary>
/// Provides deterministic UTC timestamps for messages.
/// </summary>
public interface IMessageTimestampProvider
{
    DateTime UtcNow { get; }
}
