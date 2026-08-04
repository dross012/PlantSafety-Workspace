// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           CraneHoistAssetType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Flags enumeration of crane/hoist asset classes covered by the CraneHoistSystems
//                  registry, per OSHA 1910.179 (overhead/gantry cranes) and 1926.1400-series
//                  (mobile/construction cranes and derricks).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;

/// <summary>Crane/hoist asset classes covered by the CraneHoistSystems registry.</summary>
[Flags]
public enum CraneHoistAssetType
{
    /// <summary>No asset type recorded.</summary>
    None = 0,

    /// <summary>Overhead (bridge) crane.</summary>
    OverheadBridgeCrane = 1 << 0,

    /// <summary>Gantry crane.</summary>
    GantryCrane = 1 << 1,

    /// <summary>Jib crane.</summary>
    JibCrane = 1 << 2,

    /// <summary>Mobile or construction crane (OSHA 1926.1400-series).</summary>
    MobileCrane = 1 << 3,

    /// <summary>Powered hoist not attached to a bridge/gantry/jib structure.</summary>
    Hoist = 1 << 4,
}
