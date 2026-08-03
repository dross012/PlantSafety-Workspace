// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.MultiRegion
//  Layer:          SharedKernel
//  File:           RegionHealthStatus.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 §4.3 (Global Routing) Simple Enum representing the current health
//                  classification of a region, as an input to IRegionFailoverPolicy. A Simple
//                  Enum (not a Smart Enum) per Handbook Rule 2.7.B -- this carries no behavior
//                  or metadata of its own; it is a plain classification value produced by
//                  whatever health-signal source is wired up (today: no live cross-region
//                  monitoring exists in this sandbox, so callers must supply this explicitly --
//                  see GeoReplicationHealthCheck for the honest, current-state placeholder).
// ============================================================================
namespace PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;

/// <summary>Health classification of a region, used as input to failover decisions.</summary>
public enum RegionHealthStatus
{
    /// <summary>The region is fully operational.</summary>
    Healthy = 0,

    /// <summary>The region is operational but degraded (e.g. elevated latency, replication lag above target).</summary>
    Degraded = 1,

    /// <summary>The region is not operational and must not receive routed traffic.</summary>
    Unavailable = 2,
}
