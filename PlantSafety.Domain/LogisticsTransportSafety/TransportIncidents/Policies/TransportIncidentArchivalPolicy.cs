// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Domain
//  File:           TransportIncidentArchivalPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain policy governing archival eligibility for TransportIncident aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Policies;

/// <summary>Determines whether a <see cref="TransportIncident"/> aggregate is eligible for archival.</summary>
public static class TransportIncidentArchivalPolicy
{
    /// <summary>Returns <see langword="true"/> when the incident record may be archived (closed only).</summary>
    public static bool IsEligibleForArchival(PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident aggregate)
        => aggregate.Status == TransportIncidentStatus.Closed;
}
