// ============================================================================
//  Project:        Deployment
//  Module:         Boundaries
//  File:           ModuleBoundaryConfig.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. The deterministic tenant/module/
//                  facility/department/machine-group boundary a deployment
//                  or configuration change must stay within.
//                  ModuleBoundaryChecker performs explicit, field-by-field
//                  containment checks -- never reflection, never a generic
//                  "does this object match this filter" walker.
// ============================================================================
namespace Deployment.Boundaries;

/// <summary>The deterministic set of scoping boundaries a single tenant is confined to.</summary>
public sealed record ModuleBoundaryConfig(
    string TenantId,
    IReadOnlyList<string> AllowedModuleIds,
    IReadOnlyList<string> AllowedFacilityIds,
    IReadOnlyList<string> AllowedDepartmentIds,
    IReadOnlyList<string> AllowedMachineGroupIds);

/// <summary>Deterministic, pure containment checks against a ModuleBoundaryConfig.</summary>
public static class ModuleBoundaryChecker
{
    /// <summary>Returns true if <paramref name="moduleId"/> is within <paramref name="boundary"/>'s AllowedModuleIds.</summary>
    public static bool IsModuleAllowed(ModuleBoundaryConfig boundary, string moduleId)
    {
        ArgumentNullException.ThrowIfNull(boundary);
        ArgumentNullException.ThrowIfNull(moduleId);

        return boundary.AllowedModuleIds.Contains(moduleId, StringComparer.Ordinal);
    }

    /// <summary>Returns true if <paramref name="facilityId"/> is within <paramref name="boundary"/>'s AllowedFacilityIds.</summary>
    public static bool IsFacilityAllowed(ModuleBoundaryConfig boundary, string facilityId)
    {
        ArgumentNullException.ThrowIfNull(boundary);
        ArgumentNullException.ThrowIfNull(facilityId);

        return boundary.AllowedFacilityIds.Contains(facilityId, StringComparer.Ordinal);
    }

    /// <summary>Returns true if <paramref name="departmentId"/> is within <paramref name="boundary"/>'s AllowedDepartmentIds.</summary>
    public static bool IsDepartmentAllowed(ModuleBoundaryConfig boundary, string departmentId)
    {
        ArgumentNullException.ThrowIfNull(boundary);
        ArgumentNullException.ThrowIfNull(departmentId);

        return boundary.AllowedDepartmentIds.Contains(departmentId, StringComparer.Ordinal);
    }

    /// <summary>Returns true if <paramref name="machineGroupId"/> is within <paramref name="boundary"/>'s AllowedMachineGroupIds.</summary>
    public static bool IsMachineGroupAllowed(ModuleBoundaryConfig boundary, string machineGroupId)
    {
        ArgumentNullException.ThrowIfNull(boundary);
        ArgumentNullException.ThrowIfNull(machineGroupId);

        return boundary.AllowedMachineGroupIds.Contains(machineGroupId, StringComparer.Ordinal);
    }

    /// <summary>
    /// Returns true only if every one of <paramref name="moduleId"/>, <paramref name="facilityId"/>,
    /// <paramref name="departmentId"/>, and <paramref name="machineGroupId"/> that is supplied
    /// (non-null) is within <paramref name="boundary"/>. A null argument is treated as "not scoped by
    /// this dimension" and is not checked.
    /// </summary>
    public static bool IsWithinBoundary(
        ModuleBoundaryConfig boundary,
        string? moduleId = null,
        string? facilityId = null,
        string? departmentId = null,
        string? machineGroupId = null)
    {
        ArgumentNullException.ThrowIfNull(boundary);

        if (moduleId is not null && !IsModuleAllowed(boundary, moduleId))
        {
            return false;
        }

        if (facilityId is not null && !IsFacilityAllowed(boundary, facilityId))
        {
            return false;
        }

        if (departmentId is not null && !IsDepartmentAllowed(boundary, departmentId))
        {
            return false;
        }

        if (machineGroupId is not null && !IsMachineGroupAllowed(boundary, machineGroupId))
        {
            return false;
        }

        return true;
    }
}
