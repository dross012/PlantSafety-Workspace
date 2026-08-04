// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.CyberPhysicalIncident
//  Layer:          Domain
//  File:           CyberPhysicalIncidentEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity for CyberPhysicalIncident domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for CyberPhysicalIncident domain events.</summary>
public readonly record struct CyberPhysicalIncidentEventId(Guid Value)
{
    /// <summary>Creates a new CyberPhysicalIncidentEventId using Uuid7.</summary>
    public static CyberPhysicalIncidentEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
