// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Domain
//  File:           TransportIncidentLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the TransportIncident aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="TransportIncident"/> aggregate.</summary>
public static class TransportIncidentLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when investigation may begin from <paramref name="current"/>.</summary>
    public static bool CanBeginInvestigation(TransportIncidentStatus current) => current == TransportIncidentStatus.Reported;

    /// <summary>Returns <see langword="true"/> when the incident may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(TransportIncidentStatus current) => current == TransportIncidentStatus.UnderInvestigation;
}
