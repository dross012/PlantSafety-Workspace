// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.MachineGuardsBarriers
//  Layer:          Domain
//  File:           MachineGuardsBarriersLifecycleRules.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the MachineGuardsBarriers aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.MachineGuardsBarriers.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="MachineGuardsBarriers"/> aggregate.</summary>
public static class MachineGuardsBarriersLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when an inspection may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleInspection(MachineGuardsBarriersStatus current) =>
        current is MachineGuardsBarriersStatus.Installed or MachineGuardsBarriersStatus.Compliant;

    /// <summary>Returns <see langword="true"/> when an inspection result may be recorded from <paramref name="current"/>.</summary>
    public static bool CanRecordInspectionResult(MachineGuardsBarriersStatus current) => current == MachineGuardsBarriersStatus.InspectionScheduled;

    /// <summary>Returns <see langword="true"/> when corrective action may begin from <paramref name="current"/>.</summary>
    public static bool CanBeginCorrectiveAction(MachineGuardsBarriersStatus current) => current == MachineGuardsBarriersStatus.DeficiencyFound;

    /// <summary>Returns <see langword="true"/> when corrective action may be verified from <paramref name="current"/>.</summary>
    public static bool CanVerifyCorrectiveAction(MachineGuardsBarriersStatus current) => current == MachineGuardsBarriersStatus.CorrectiveActionInProgress;

    /// <summary>Returns <see langword="true"/> when the guard may be retired from <paramref name="current"/>.</summary>
    public static bool CanRetire(MachineGuardsBarriersStatus current) => current != MachineGuardsBarriersStatus.Retired;
}
