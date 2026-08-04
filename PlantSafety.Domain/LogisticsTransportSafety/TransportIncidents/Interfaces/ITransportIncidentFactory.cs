// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Domain
//  File:           ITransportIncidentFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing TransportIncident aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Interfaces;

/// <summary>Factory interface for constructing <see cref="TransportIncident"/> aggregates.</summary>
public interface ITransportIncidentFactory
{
    /// <summary>Creates a new TransportIncident aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
