// ============================================================================
//  Project:        Shared.MachineDomain
//  Module:         Abstractions.Maintenance
//  File:           MachineMaintenanceProfile.cs
//  Created:        2026-08-08
//  Description:    Phase C.2: a machine's maintenance schedule state.
//                  Replaced wholesale (not incrementally merged) by
//                  MachineAggregate.UpdateMaintenanceProfile, mirroring
//                  Shared.FacilityDomain's replace-not-merge convention.
// ============================================================================
namespace Shared.MachineDomain.Abstractions.Maintenance;

/// <summary>A machine's maintenance schedule state, as of <see cref="UpdatedAtUtc"/>.</summary>
public sealed record MachineMaintenanceProfile(
    DateTime? LastMaintenanceAtUtc,
    DateTime? NextScheduledMaintenanceAtUtc,
    int MaintenanceIntervalDays,
    DateTime UpdatedAtUtc)
{
    /// <summary>True if the next scheduled maintenance date has already passed, relative to <paramref name="nowUtc"/>.</summary>
    public bool IsOverdue(DateTime nowUtc) => NextScheduledMaintenanceAtUtc is not null && NextScheduledMaintenanceAtUtc.Value <= nowUtc;
}
