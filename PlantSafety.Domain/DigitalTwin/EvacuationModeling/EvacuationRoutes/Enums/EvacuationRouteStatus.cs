// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationRoutes
//  Layer:          Domain
//  File:           EvacuationRouteStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Real lifecycle states for the EvacuationRoute aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationRoutes.Enums;

/// <summary>Lifecycle states for the <see cref="EvacuationRoute"/> aggregate root.</summary>
public enum EvacuationRouteStatus
{
    /// <summary>Route defined and clear of obstructions.</summary>
    Active = 0,

    /// <summary>Route currently has one or more open obstructions.</summary>
    Obstructed = 1,

    /// <summary>Route permanently retired/decommissioned.</summary>
    Retired = 2,
}
