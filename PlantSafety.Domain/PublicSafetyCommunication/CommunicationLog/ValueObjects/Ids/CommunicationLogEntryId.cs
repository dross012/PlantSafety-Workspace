// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           CommunicationLogEntryId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the CommunicationLogEntry aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the CommunicationLogEntry aggregate root.</summary>
public readonly record struct CommunicationLogEntryId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CommunicationLogEntryId wrapping a non-empty GUID.</summary>
    public CommunicationLogEntryId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommunicationLogEntryId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CommunicationLogEntryId using Uuid7.</summary>
    public static CommunicationLogEntryId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CommunicationLogEntryId.</summary>
    public static CommunicationLogEntryId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a CommunicationLogEntryId.</summary>
    public static CommunicationLogEntryId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
