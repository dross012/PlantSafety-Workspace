// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Domain
//  File:           GlobalSafetyRiskScopeType.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §2 Multi-Site Safety Intelligence Network)
//  SchemaVersion:  2026.1
//  Description:    Simple enum identifying which dimension a SiteRiskScore was aggregated over.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.GlobalSafetyInsights.Enums;

/// <summary>Dimension a <see cref="ValueObjects.SiteRiskScore"/> was aggregated over.</summary>
public enum GlobalSafetyRiskScopeType
{
    /// <summary>Score aggregated for a single SiteId.</summary>
    Site = 0,

    /// <summary>Score aggregated across every site within a Region.</summary>
    Region = 1,

    /// <summary>Score aggregated across a named machine family, across sites.</summary>
    MachineFamily = 2,
}
