// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Checklists
//  Layer:          Domain
//  File:           ChecklistCategory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Smart Enum enumerating the 20 canonical manufacturing safety
//                  checklist categories used across the plant (per PlantSafety
//                  Program Structure, "Comprehensive Manufacturing Safety
//                  Checklists"). Drives ChecklistTemplate classification,
//                  dashboard grouping, and cross-module reporting; therefore
//                  modeled as a Smart Enum (Rule 2.7) rather than a plain enum.
//                  Additive-only evolution: new categories may be appended;
//                  existing codes/ids/display names must never change meaning.
// ============================================================================
namespace PlantSafety.Domain.Checklists.SmartEnums;

/// <summary>
/// Canonical manufacturing safety checklist category. Strong-typed Smart Enum with
/// stable <see cref="Id"/>/<see cref="Code"/> pairs so downstream systems (dashboards,
/// audits, regulatory mappings) can reference categories without magic strings.
/// </summary>
public sealed class ChecklistCategory
{
    /// <summary>Stable numeric identifier (1-based, matches the source document ordering).</summary>
    public int Id { get; }

    /// <summary>Stable machine-readable code (PascalCase, no abbreviations).</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    private ChecklistCategory(int id, string code, string displayName)
    {
        Id = id;
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Machine Guarding &amp; LOTO Checklists.</summary>
    public static readonly ChecklistCategory MachineGuardingAndLoto = new(1, "MachineGuardingAndLoto", "Machine Guarding & LOTO Checklists");

    /// <summary>Personal Protective Equipment (PPE) Checklists.</summary>
    public static readonly ChecklistCategory PersonalProtectiveEquipment = new(2, "PersonalProtectiveEquipment", "Personal Protective Equipment (PPE) Checklists");

    /// <summary>Hazardous Materials &amp; Chemical Safety Checklists.</summary>
    public static readonly ChecklistCategory HazardousMaterialsAndChemicalSafety = new(3, "HazardousMaterialsAndChemicalSafety", "Hazardous Materials & Chemical Safety Checklists");

    /// <summary>Emergency Systems &amp; Life Safety Checklists.</summary>
    public static readonly ChecklistCategory EmergencySystemsAndLifeSafety = new(4, "EmergencySystemsAndLifeSafety", "Emergency Systems & Life Safety Checklists");

    /// <summary>Environmental &amp; Housekeeping Checklists.</summary>
    public static readonly ChecklistCategory EnvironmentalAndHousekeeping = new(5, "EnvironmentalAndHousekeeping", "Environmental & Housekeeping Checklists");

    /// <summary>Material Handling Equipment Checklists.</summary>
    public static readonly ChecklistCategory MaterialHandlingEquipment = new(6, "MaterialHandlingEquipment", "Material Handling Equipment Checklists");

    /// <summary>Electrical Safety Checklists.</summary>
    public static readonly ChecklistCategory ElectricalSafety = new(7, "ElectricalSafety", "Electrical Safety Checklists");

    /// <summary>Confined Space Entry Checklists.</summary>
    public static readonly ChecklistCategory ConfinedSpaceEntry = new(8, "ConfinedSpaceEntry", "Confined Space Entry Checklists");

    /// <summary>Hot Work Checklists.</summary>
    public static readonly ChecklistCategory HotWork = new(9, "HotWork", "Hot Work Checklists");

    /// <summary>Working at Heights Checklists.</summary>
    public static readonly ChecklistCategory WorkingAtHeights = new(10, "WorkingAtHeights", "Working at Heights Checklists");

    /// <summary>Contractor Safety Checklists.</summary>
    public static readonly ChecklistCategory ContractorSafety = new(11, "ContractorSafety", "Contractor Safety Checklists");

    /// <summary>Machine Maintenance &amp; PM Checklists.</summary>
    public static readonly ChecklistCategory MachineMaintenanceAndPm = new(12, "MachineMaintenanceAndPm", "Machine Maintenance & PM Checklists");

    /// <summary>Facility &amp; Traffic Control Checklists.</summary>
    public static readonly ChecklistCategory FacilityAndTrafficControl = new(13, "FacilityAndTrafficControl", "Facility & Traffic Control Checklists");

    /// <summary>Fire Safety &amp; Prevention Checklists.</summary>
    public static readonly ChecklistCategory FireSafetyAndPrevention = new(14, "FireSafetyAndPrevention", "Fire Safety & Prevention Checklists");

    /// <summary>Ergonomics &amp; Workstation Checklists.</summary>
    public static readonly ChecklistCategory ErgonomicsAndWorkstation = new(15, "ErgonomicsAndWorkstation", "Ergonomics & Workstation Checklists");

    /// <summary>Incident &amp; Near-Miss Reporting Checklists.</summary>
    public static readonly ChecklistCategory IncidentAndNearMissReporting = new(16, "IncidentAndNearMissReporting", "Incident & Near-Miss Reporting Checklists");

    /// <summary>Environmental Compliance Checklists.</summary>
    public static readonly ChecklistCategory EnvironmentalCompliance = new(17, "EnvironmentalCompliance", "Environmental Compliance Checklists");

    /// <summary>Security &amp; Access Control Checklists.</summary>
    public static readonly ChecklistCategory SecurityAndAccessControl = new(18, "SecurityAndAccessControl", "Security & Access Control Checklists");

    /// <summary>Emergency Drill Checklists.</summary>
    public static readonly ChecklistCategory EmergencyDrill = new(19, "EmergencyDrill", "Emergency Drill Checklists");

    /// <summary>Observability &amp; Sensor Health Checklists.</summary>
    public static readonly ChecklistCategory ObservabilityAndSensorHealth = new(20, "ObservabilityAndSensorHealth", "Observability & Sensor Health Checklists");

    /// <summary>Default category used when no explicit category has been assigned.</summary>
    public static readonly ChecklistCategory Default = MachineGuardingAndLoto;

    /// <summary>All defined checklist categories, in canonical (source-document) order.</summary>
    public static readonly IReadOnlyList<ChecklistCategory> All =
    [
        MachineGuardingAndLoto,
        PersonalProtectiveEquipment,
        HazardousMaterialsAndChemicalSafety,
        EmergencySystemsAndLifeSafety,
        EnvironmentalAndHousekeeping,
        MaterialHandlingEquipment,
        ElectricalSafety,
        ConfinedSpaceEntry,
        HotWork,
        WorkingAtHeights,
        ContractorSafety,
        MachineMaintenanceAndPm,
        FacilityAndTrafficControl,
        FireSafetyAndPrevention,
        ErgonomicsAndWorkstation,
        IncidentAndNearMissReporting,
        EnvironmentalCompliance,
        SecurityAndAccessControl,
        EmergencyDrill,
        ObservabilityAndSensorHealth,
    ];

    /// <summary>Resolves a <see cref="ChecklistCategory"/> from its <see cref="Code"/>. Throws if not found.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="code"/> does not match a known category.</exception>
    public static ChecklistCategory From(string code)
        => TryParse(code, out var value)
            ? value!
            : throw new ArgumentException($"'{code}' is not a valid ChecklistCategory code.", nameof(code));

    /// <summary>Attempts to resolve a <see cref="ChecklistCategory"/> from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out ChecklistCategory? value)
    {
        value = All.FirstOrDefault(c => string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognised category code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
