// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Domain
//  File:           TransportIncidentType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Classification of transport incident types.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Enums;

/// <summary>Classification of transport incident types.</summary>
public enum TransportIncidentType
{
    /// <summary>Vehicle collision.</summary>
    Collision = 0,

    /// <summary>Cargo spill or load loss.</summary>
    CargoSpill = 1,

    /// <summary>Near-miss with no contact or loss.</summary>
    NearMiss = 2,

    /// <summary>Vehicle rollover or tip-over.</summary>
    RolloverOrTipOver = 3,

    /// <summary>Interaction with a pedestrian.</summary>
    PedestrianInteraction = 4,
}
