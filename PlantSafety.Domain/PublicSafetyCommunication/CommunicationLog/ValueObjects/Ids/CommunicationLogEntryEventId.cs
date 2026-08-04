// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           CommunicationLogEntryEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for CommunicationLogEntry domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for CommunicationLogEntry domain events.</summary>
public readonly record struct CommunicationLogEntryEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CommunicationLogEntryEventId wrapping a non-empty GUID.</summary>
    public CommunicationLogEntryEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommunicationLogEntryEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CommunicationLogEntryEventId using Uuid7.</summary>
    public static CommunicationLogEntryEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
