// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           ProductionLineStatus.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Real-time operating status for the ProductionLine master-data aggregate.
//                  Plain simple enum (Handbook Rule 2.7B), mirroring the shape of the existing
//                  Observability.Telemetry.Enums.MachineOperatingState precedent at the
//                  coarser line-level granularity. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.ProductionTracking.Enums;

/// <summary>Real-time operating status of a ProductionLine.</summary>
public enum ProductionLineStatus
{
    /// <summary>Line is actively running one or more production runs.</summary>
    Active = 0,

    /// <summary>Line is powered and available but currently idle (no active run).</summary>
    Idle = 1,

    /// <summary>Line is down (unplanned) or out of service (planned maintenance).</summary>
    Down = 2,
}
