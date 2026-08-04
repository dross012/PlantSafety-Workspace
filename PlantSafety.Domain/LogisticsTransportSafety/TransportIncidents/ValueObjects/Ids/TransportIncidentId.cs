// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Domain
//  File:           TransportIncidentId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the TransportIncident aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the TransportIncident aggregate root.</summary>
public readonly record struct TransportIncidentId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a TransportIncidentId wrapping a non-empty GUID.</summary>
    public TransportIncidentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TransportIncidentId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new TransportIncidentId using Uuid7.</summary>
    public static TransportIncidentId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a TransportIncidentId.</summary>
    public static TransportIncidentId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a TransportIncidentId.</summary>
    public static TransportIncidentId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
