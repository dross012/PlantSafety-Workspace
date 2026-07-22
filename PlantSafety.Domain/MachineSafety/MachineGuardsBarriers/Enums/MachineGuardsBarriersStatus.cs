// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Domain
//  File:           MachineGuardsBarriersStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real inspection lifecycle states for the MachineGuardsBarriers aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Enums;

/// <summary>Lifecycle states for the <see cref="MachineGuardsBarriers"/> aggregate root.</summary>
public enum MachineGuardsBarriersStatus
{
    /// <summary>Guard/barrier installed and awaiting first inspection.</summary>
    Installed = 0,

    /// <summary>Inspection scheduled and pending.</summary>
    InspectionScheduled = 1,

    /// <summary>Guard passed inspection; compliant and in service.</summary>
    Compliant = 2,

    /// <summary>Deficiency found during inspection; corrective action required.</summary>
    DeficiencyFound = 3,

    /// <summary>Corrective action underway to remediate a deficiency.</summary>
    CorrectiveActionInProgress = 4,

    /// <summary>Guard/barrier permanently retired.</summary>
    Retired = 5,
}
