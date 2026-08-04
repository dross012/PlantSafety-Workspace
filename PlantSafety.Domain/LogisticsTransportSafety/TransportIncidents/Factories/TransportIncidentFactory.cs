// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Domain
//  File:           TransportIncidentFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing TransportIncident aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Factories;

/// <summary>Default factory implementation for constructing <see cref="TransportIncident"/> aggregates.</summary>
public sealed class TransportIncidentFactory : ITransportIncidentFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates.TransportIncident.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
