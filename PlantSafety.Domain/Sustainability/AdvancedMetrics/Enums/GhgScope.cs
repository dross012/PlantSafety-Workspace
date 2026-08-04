// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           GhgScope.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    GHG Protocol emissions scope classification for a CarbonEmissionRecord.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;

/// <summary>GHG Protocol emissions scope classification.</summary>
public enum GhgScope
{
    /// <summary>Scope 1: direct emissions from owned or controlled sources.</summary>
    Scope1 = 0,

    /// <summary>Scope 2: indirect emissions from purchased electricity, steam, heating, and cooling.</summary>
    Scope2 = 1,

    /// <summary>Scope 3: all other indirect emissions in the value chain (upstream and downstream).</summary>
    Scope3 = 2,
}
