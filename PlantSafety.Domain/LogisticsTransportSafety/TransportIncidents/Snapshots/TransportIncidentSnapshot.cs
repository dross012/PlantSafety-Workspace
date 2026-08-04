// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Domain
//  File:           TransportIncidentSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for TransportIncident aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Snapshots;

/// <summary>Immutable snapshot of a <see cref="TransportIncident"/> aggregate for fast rehydration.</summary>
public sealed record TransportIncidentSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public TransportIncidentId TransportIncidentId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public TransportIncidentStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
