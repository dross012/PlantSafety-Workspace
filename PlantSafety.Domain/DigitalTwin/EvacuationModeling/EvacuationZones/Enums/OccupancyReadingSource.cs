// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           OccupancyReadingSource.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Source classification for a recorded occupancy reading. Data-governance note:
//                  occupancy data is aggregate/anonymized at the point of ingestion regardless of
//                  source (no individually identifiable headcount data is retained on the reading).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Enums;

/// <summary>Source classification for a recorded occupancy reading.</summary>
public enum OccupancyReadingSource
{
    /// <summary>Reading sourced from a Building Management System feed (Infrastructure.Integrations).</summary>
    BmsFeed = 0,

    /// <summary>Reading sourced from a dedicated occupancy sensor.</summary>
    Sensor = 1,

    /// <summary>Reading manually entered/estimated by a user.</summary>
    Manual = 2,

    /// <summary>Reading derived from an egress simulation run.</summary>
    SimulationDerived = 3,
}
