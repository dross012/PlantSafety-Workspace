// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           CraneHoistSystemsStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Lifecycle states for the CraneHoistSystems aggregate root (crane/hoist asset
//                  registry entry covering registration, in-service operation, the mandatory
//                  periodic-inspection cycle, and retirement).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;

/// <summary>Lifecycle states for the <see cref="CraneHoistSystems"/> aggregate root.</summary>
public enum CraneHoistSystemsStatus
{
    /// <summary>Asset registered in the crane/hoist registry but not yet placed in service.</summary>
    Registered = 0,

    /// <summary>Asset in normal service with a current inspection.</summary>
    InService = 1,

    /// <summary>An inspection has been scheduled and is not yet due.</summary>
    InspectionDue = 2,

    /// <summary>The scheduled inspection date has passed without a recorded completion.</summary>
    InspectionOverdue = 3,

    /// <summary>Asset removed from service pending corrective action (e.g. repeated overloads or rigging defects).</summary>
    Suspended = 4,

    /// <summary>Asset permanently retired from the crane/hoist registry.</summary>
    Retired = 5,
}
