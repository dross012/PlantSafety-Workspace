// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           OtAssetType.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Classification of an OT/ICS asset within the plant network.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Enums;

/// <summary>Classification of an OT/ICS asset.</summary>
public enum OtAssetType
{
    /// <summary>Programmable Logic Controller.</summary>
    Plc = 0,

    /// <summary>Human-Machine Interface.</summary>
    Hmi = 1,

    /// <summary>Supervisory Control and Data Acquisition system.</summary>
    Scada = 2,

    /// <summary>Remote Terminal Unit.</summary>
    Rtu = 3,

    /// <summary>Process historian / data logging system.</summary>
    Historian = 4,

    /// <summary>OT network infrastructure device (switch, firewall, gateway).</summary>
    NetworkDevice = 5,

    /// <summary>Physical sensor instrument.</summary>
    Sensor = 6,

    /// <summary>Physical actuator/final control element.</summary>
    Actuator = 7,

    /// <summary>Any other OT/ICS asset not covered above.</summary>
    Other = 8
}
