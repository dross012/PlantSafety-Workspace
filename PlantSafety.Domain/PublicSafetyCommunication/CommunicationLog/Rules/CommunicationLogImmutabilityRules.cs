// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           CommunicationLogImmutabilityRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Documents and enforces the immutability invariant for CommunicationLogEntry:
//                  once recorded, a log entry has no permitted mutating lifecycle transitions.
//                  The CommunicationLogEntry aggregate deliberately exposes no update methods;
//                  this rule class exists so infrastructure/application callers have an explicit,
//                  discoverable guard to invoke before ever attempting a write-path update.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Rules;

/// <summary>Enforces the immutability invariant for the <see cref="CommunicationLogEntry"/> aggregate.</summary>
public static class CommunicationLogImmutabilityRules
{
    /// <summary>Always returns <see langword="false"/> -- no mutation of a recorded CommunicationLogEntry is ever permitted.</summary>
    public static bool CanMutate() => false;

    /// <summary>Throws <see cref="CommunicationLogEntryImmutableException"/> for any attempted mutation.</summary>
    public static void GuardAgainstMutation(CommunicationLogEntryId id, CorrelationId correlationId, CausationId causationId)
        => throw new CommunicationLogEntryImmutableException(id, correlationId, causationId);
}
