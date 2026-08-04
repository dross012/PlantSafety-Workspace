// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Domain
//  File:           TransportIncidentStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the TransportIncident aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Enums;

/// <summary>Lifecycle states for the <see cref="TransportIncident"/> aggregate root.</summary>
public enum TransportIncidentStatus
{
    /// <summary>Incident reported and awaiting investigation.</summary>
    Reported = 0,

    /// <summary>Incident investigation in progress.</summary>
    UnderInvestigation = 1,

    /// <summary>Incident investigation closed.</summary>
    Closed = 2,
}
