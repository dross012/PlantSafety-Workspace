// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Domain
//  File:           TransportIncidentSeverity.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Severity classification for transport incidents, feeding RiskManagement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Enums;

/// <summary>Severity classification for transport incidents.</summary>
public enum TransportIncidentSeverity
{
    /// <summary>Minor incident with negligible impact.</summary>
    Minor = 0,

    /// <summary>Moderate incident requiring follow-up.</summary>
    Moderate = 1,

    /// <summary>Major incident with significant impact.</summary>
    Major = 2,

    /// <summary>Critical incident requiring immediate escalation.</summary>
    Critical = 3,
}
