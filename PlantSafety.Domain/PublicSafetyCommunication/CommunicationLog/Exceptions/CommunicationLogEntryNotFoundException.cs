// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           CommunicationLogEntryNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a CommunicationLogEntry aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Exceptions;

/// <summary>Thrown when a <see cref="CommunicationLogEntry"/> aggregate lookup finds no match.</summary>
public sealed class CommunicationLogEntryNotFoundException : CommunicationLogEntryDomainException
{
    /// <summary>Creates a not-found exception for the given CommunicationLogEntry identity.</summary>
    public CommunicationLogEntryNotFoundException(CommunicationLogEntryId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "CommunicationLogEntry with id '{id}' was not found.",
            "CommunicationLogEntry.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
