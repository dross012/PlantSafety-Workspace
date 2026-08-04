// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Domain
//  File:           TransportIncidentNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a TransportIncident aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Exceptions;

/// <summary>Thrown when a <see cref="TransportIncident"/> aggregate lookup finds no match.</summary>
public sealed class TransportIncidentNotFoundException : TransportIncidentDomainException
{
    /// <summary>Creates a not-found exception for the given TransportIncident identity.</summary>
    public TransportIncidentNotFoundException(TransportIncidentId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"TransportIncident with id '{id}' was not found.",
            "TransportIncident.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
