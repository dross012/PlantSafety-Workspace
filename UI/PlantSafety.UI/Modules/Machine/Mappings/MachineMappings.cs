// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.Machine.Mappings
//  File:           MachineMappings.cs
//  Created:        2026-08-09
//  Description:    Backend DTO mirror + UI ViewModel + explicit mapping for
//                  the Machine module. See FacilityMappings.cs for the same
//                  "no project reference to Shared.MachineDomain" rationale.
// ============================================================================
namespace PlantSafety.UI.Modules.Machine.Mappings;

/// <summary>Local mirror of the wire shape PlantSafety.UI.Api's MachinesController returns.</summary>
public sealed record MachineBackendDto(
    Guid MachineId,
    string Name,
    string MachineType,
    MachineLocationDto Location,
    MachineMaintenanceProfileDto MaintenanceProfile,
    IReadOnlyList<object> FailureRecords,
    MachineSafetyScoreDto SafetyScore,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

/// <summary>Local mirror of MachineLocation.</summary>
public sealed record MachineLocationDto(Guid FacilityId, string AreaDescription);

/// <summary>Local mirror of MachineMaintenanceProfile.</summary>
public sealed record MachineMaintenanceProfileDto(DateTime? LastMaintenanceAtUtc, DateTime? NextScheduledMaintenanceAtUtc, int MaintenanceIntervalDays);

/// <summary>Local mirror of MachineSafetyScore.</summary>
public sealed record MachineSafetyScoreDto(double Score);

/// <summary>The UI-facing shape of a machine, flattened for display.</summary>
public sealed record MachineViewModel(
    Guid MachineId,
    string Name,
    string MachineType,
    Guid FacilityId,
    string AreaDescription,
    DateTime? NextScheduledMaintenanceAtUtc,
    int FailureRecordCount,
    double SafetyScore,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

/// <summary>Explicit, one-directional Backend DTO -&gt; UI ViewModel mapping.</summary>
public static class MachineMappings
{
    /// <summary>Maps a backend MachineBackendDto snapshot to a UI-facing MachineViewModel.</summary>
    public static MachineViewModel ToViewModel(this MachineBackendDto dto) => new(
        dto.MachineId,
        dto.Name,
        dto.MachineType,
        dto.Location.FacilityId,
        dto.Location.AreaDescription,
        dto.MaintenanceProfile.NextScheduledMaintenanceAtUtc,
        dto.FailureRecords.Count,
        dto.SafetyScore.Score,
        dto.CreatedAtUtc,
        dto.UpdatedAtUtc);
}
