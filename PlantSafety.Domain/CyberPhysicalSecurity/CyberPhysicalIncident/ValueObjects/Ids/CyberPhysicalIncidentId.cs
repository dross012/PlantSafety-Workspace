// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.CyberPhysicalIncident
//  Layer:          Domain
//  File:           CyberPhysicalIncidentId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for the CyberPhysicalIncident aggregate root (Incident
//                  Response subdomain of CyberPhysicalSecurity).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the CyberPhysicalIncident aggregate root.</summary>
public readonly record struct CyberPhysicalIncidentId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a CyberPhysicalIncidentId wrapping a non-empty GUID.</summary>
    public CyberPhysicalIncidentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CyberPhysicalIncidentId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new CyberPhysicalIncidentId using Uuid7.</summary>
    public static CyberPhysicalIncidentId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a CyberPhysicalIncidentId.</summary>
    public static CyberPhysicalIncidentId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
