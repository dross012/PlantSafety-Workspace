// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Domain
//  File:           TransportIncidentEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for TransportIncident domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for TransportIncident domain events.</summary>
public readonly record struct TransportIncidentEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a TransportIncidentEventId wrapping a non-empty GUID.</summary>
    public TransportIncidentEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TransportIncidentEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new TransportIncidentEventId using Uuid7.</summary>
    public static TransportIncidentEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
