// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Domain
//  File:           LotoProgramsStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real lifecycle states for a LOTO program instance: documentation, isolation
//                  application, zero-energy verification, release, and non-compliance.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.LotoPrograms.Enums;

/// <summary>Lifecycle states for the <see cref="LotoPrograms"/> aggregate root.</summary>
public enum LotoProgramsStatus
{
    /// <summary>Program documented: energy sources and isolation points identified, not yet applied.</summary>
    Documented = 0,

    /// <summary>Locks, hasps, and tags have been applied to all identified isolation points.</summary>
    IsolationApplied = 1,

    /// <summary>Zero-energy state has been verified by an authorized employee.</summary>
    ZeroEnergyVerified = 2,

    /// <summary>Isolation devices have been removed and the equipment released back to service.</summary>
    Released = 3,

    /// <summary>Program found non-compliant during an audit (e.g. missing isolation point, unauthorized removal).</summary>
    NonCompliant = 4,
}
