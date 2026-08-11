// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           PolicyCategory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the SafetyPolicies bounded context. Carries SafetyPolicies-specific category metadata and drives downstream workflow, compliance, and reporting behavior per Handbook Rule 2.7.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyPolicies.Enums;

/// <summary>Smart Enum representing the governing category taxonomy for SafetyPolicies.</summary>
public sealed class PolicyCategory : IEquatable<PolicyCategory>
{
    /// <summary>Hazard Communication &amp; Assessment Policies</summary>
    public static readonly PolicyCategory HazardCommunicationAssessment = new("HazardCommunicationAssessment", "Hazard Communication & Assessment Policies");

    /// <summary>Equipment &amp; Machine Operation Policies</summary>
    public static readonly PolicyCategory EquipmentMachineOperation = new("EquipmentMachineOperation", "Equipment & Machine Operation Policies");

    /// <summary>Personal Protective Equipment (PPE) Policies</summary>
    public static readonly PolicyCategory PersonalProtectiveEquipment = new("PersonalProtectiveEquipment", "Personal Protective Equipment (PPE) Policies");

    /// <summary>Emergency Response &amp; Evacuation Policies</summary>
    public static readonly PolicyCategory EmergencyResponseEvacuation = new("EmergencyResponseEvacuation", "Emergency Response & Evacuation Policies");

    /// <summary>Housekeeping &amp; Ergonomics Policies</summary>
    public static readonly PolicyCategory HousekeepingErgonomics = new("HousekeepingErgonomics", "Housekeeping & Ergonomics Policies");

    /// <summary>Safety Training &amp; Reporting Policies</summary>
    public static readonly PolicyCategory SafetyTrainingReporting = new("SafetyTrainingReporting", "Safety Training & Reporting Policies");

    /// <summary>Contractor Safety Policies</summary>
    public static readonly PolicyCategory ContractorSafety = new("ContractorSafety", "Contractor Safety Policies");

    /// <summary>Environmental &amp; Chemical Safety Policies</summary>
    public static readonly PolicyCategory EnvironmentalChemicalSafety = new("EnvironmentalChemicalSafety", "Environmental & Chemical Safety Policies");

    /// <summary>Electrical Safety Policies</summary>
    public static readonly PolicyCategory ElectricalSafety = new("ElectricalSafety", "Electrical Safety Policies");

    /// <summary>Confined Space Entry Policies</summary>
    public static readonly PolicyCategory ConfinedSpaceEntry = new("ConfinedSpaceEntry", "Confined Space Entry Policies");

    /// <summary>Working at Heights Policies</summary>
    public static readonly PolicyCategory WorkingAtHeights = new("WorkingAtHeights", "Working at Heights Policies");

    /// <summary>Hot Work Policies</summary>
    public static readonly PolicyCategory HotWork = new("HotWork", "Hot Work Policies");

    /// <summary>Material Handling &amp; Storage Policies</summary>
    public static readonly PolicyCategory MaterialHandlingStorage = new("MaterialHandlingStorage", "Material Handling & Storage Policies");

    /// <summary>Machine Maintenance &amp; Inspection Policies</summary>
    public static readonly PolicyCategory MachineMaintenanceInspection = new("MachineMaintenanceInspection", "Machine Maintenance & Inspection Policies");

    /// <summary>Security &amp; Access Control Policies</summary>
    public static readonly PolicyCategory SecurityAccessControl = new("SecurityAccessControl", "Security & Access Control Policies");

    /// <summary>Incident Investigation &amp; Corrective Action Policies</summary>
    public static readonly PolicyCategory IncidentInvestigationCorrectiveAction = new("IncidentInvestigationCorrectiveAction", "Incident Investigation & Corrective Action Policies");

    /// <summary>Safety Culture &amp; Behavioral Policies</summary>
    public static readonly PolicyCategory SafetyCultureBehavioral = new("SafetyCultureBehavioral", "Safety Culture & Behavioral Policies");

    /// <summary>All defined PolicyCategory instances.</summary>
    public static readonly IReadOnlyList<PolicyCategory> All = [HazardCommunicationAssessment, EquipmentMachineOperation, PersonalProtectiveEquipment, EmergencyResponseEvacuation, HousekeepingErgonomics, SafetyTrainingReporting, ContractorSafety, EnvironmentalChemicalSafety, ElectricalSafety, ConfinedSpaceEntry, WorkingAtHeights, HotWork, MaterialHandlingStorage, MachineMaintenanceInspection, SecurityAccessControl, IncidentInvestigationCorrectiveAction, SafetyCultureBehavioral];

    /// <summary>The default PolicyCategory instance, used when no explicit category is supplied.</summary>
    public static PolicyCategory Default => HazardCommunicationAssessment;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    private PolicyCategory(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Resolves a PolicyCategory instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static PolicyCategory From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown PolicyCategory code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a PolicyCategory instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out PolicyCategory value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized PolicyCategory code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(PolicyCategory? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as PolicyCategory);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}