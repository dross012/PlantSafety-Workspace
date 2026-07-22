// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.LotoSystems
//  Layer:          Domain
//  File:           LotoSystemsStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real isolation lifecycle states for the LotoSystems aggregate root (machine-level
//                  energy isolation system, distinct from the EmployeeSafety.LotoPrograms procedure
//                  instance).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.LotoSystems.Enums;

/// <summary>Lifecycle states for the <see cref="LotoSystems"/> aggregate root.</summary>
public enum LotoSystemsStatus
{
    /// <summary>Isolation points documented and labeled for this machine's LOTO system.</summary>
    Documented = 0,

    /// <summary>Padlocks, hasps, and tags applied to all isolation points.</summary>
    IsolationApplied = 1,

    /// <summary>Zero-energy state verified.</summary>
    ZeroEnergyVerified = 2,

    /// <summary>Isolation released; machine returned to service.</summary>
    Released = 3,

    /// <summary>System flagged non-compliant during the mandatory annual audit.</summary>
    AuditFlagged = 4,
}
