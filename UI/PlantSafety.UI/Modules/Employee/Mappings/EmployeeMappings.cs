// ============================================================================
//  Project:        PlantSafety.UI
//  Module:         Modules.Employee.Mappings
//  File:           EmployeeMappings.cs
//  Created:        2026-08-09
//  Description:    Backend DTO mirror + UI ViewModel + explicit mapping for
//                  the Employee module. See FacilityMappings.cs for the same
//                  "no project reference to Shared.EmployeeDomain" rationale.
// ============================================================================
namespace PlantSafety.UI.Modules.Employee.Mappings;

/// <summary>Local mirror of the wire shape PlantSafety.UI.Api's EmployeesController returns.</summary>
public sealed record EmployeeBackendDto(
    Guid EmployeeId,
    string Name,
    string Role,
    EmployeeTrainingProfileDto TrainingProfile,
    IReadOnlyList<object> Certifications,
    IReadOnlyList<object> ExposureRecords,
    EmployeeInjuryProfileDto InjuryProfile,
    EmployeeSafetyScoreDto SafetyScore,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

/// <summary>Local mirror of EmployeeTrainingProfile.</summary>
public sealed record EmployeeTrainingProfileDto(IReadOnlyList<string> CompletedTrainingCodes);

/// <summary>Local mirror of EmployeeInjuryProfile.</summary>
public sealed record EmployeeInjuryProfileDto(int InjuryCount);

/// <summary>Local mirror of EmployeeSafetyScore.</summary>
public sealed record EmployeeSafetyScoreDto(double Score);

/// <summary>The UI-facing shape of an employee, flattened for display.</summary>
public sealed record EmployeeViewModel(
    Guid EmployeeId,
    string Name,
    string Role,
    int CompletedTrainingCount,
    int CertificationCount,
    int ExposureRecordCount,
    int InjuryCount,
    double SafetyScore,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

/// <summary>Explicit, one-directional Backend DTO -&gt; UI ViewModel mapping.</summary>
public static class EmployeeMappings
{
    /// <summary>Maps a backend EmployeeBackendDto snapshot to a UI-facing EmployeeViewModel.</summary>
    public static EmployeeViewModel ToViewModel(this EmployeeBackendDto dto) => new(
        dto.EmployeeId,
        dto.Name,
        dto.Role,
        dto.TrainingProfile.CompletedTrainingCodes.Count,
        dto.Certifications.Count,
        dto.ExposureRecords.Count,
        dto.InjuryProfile.InjuryCount,
        dto.SafetyScore.Score,
        dto.CreatedAtUtc,
        dto.UpdatedAtUtc);
}
