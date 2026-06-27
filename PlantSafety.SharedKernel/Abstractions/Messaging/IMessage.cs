/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               IMessage.cs
    DESCRIPTION:        Canonical message abstraction. Represents the base contract for all cross-module messages,
                        commands, events, and notifications flowing through the messaging pipeline. Enforces strong typing,
                        determinism, SIEM-safety, and additive-only evolution.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. Existing members MUST NOT change semantics.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Abstractions.Messaging;

/// <summary>
/// Base contract for all messages flowing through the messaging pipeline.
/// MUST be immutable, deterministic, and SIEM-safe.
/// </summary>
public interface IMessage
{
    /// <summary>
    /// Strong-typed message name. MUST follow Canonical Event Naming Rules.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Strong-typed version number. MUST follow Canonical Event Versioning Rules.
    /// </summary>
    int Version { get; }
}
