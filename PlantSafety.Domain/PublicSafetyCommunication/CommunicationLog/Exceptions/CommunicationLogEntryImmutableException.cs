// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           CommunicationLogEntryImmutableException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a mutation is attempted against a CommunicationLogEntry, which is
//                  immutable once recorded per the module's data-governance policy (communication
//                  logs are retained as compliance evidence and must not be altered after the
//                  fact).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Exceptions;

/// <summary>Thrown when a mutation is attempted against an already-recorded, immutable <see cref="CommunicationLogEntry"/>.</summary>
public sealed class CommunicationLogEntryImmutableException : CommunicationLogEntryDomainException
{
    /// <summary>Creates an immutability-violation exception for the given CommunicationLogEntry identity.</summary>
    public CommunicationLogEntryImmutableException(CommunicationLogEntryId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "CommunicationLogEntry '{id}' is immutable once recorded and cannot be modified.",
            "CommunicationLogEntry.Immutable",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
