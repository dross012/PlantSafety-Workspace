// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           DashboardCategory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the "FULL PROGRAM DASHBOARD LIST" (19 named dashboards) defined
//                  in PlantSafety_Program_Structure_extract.txt. Carries metadata (the owning
//                  role per "4. Role-Based Dashboards") because category drives dashboard
//                  visibility/routing — satisfying the Smart Enum requirement (2.7).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Dashboards.Dashboard.Enums;

/// <summary>Classifies a <see cref="Aggregates.Dashboard"/> into one of the platform's 19 canonical dashboard categories.</summary>
public sealed class DashboardCategory : IEquatable<DashboardCategory>
{
    /// <summary>Canonical string code for this dashboard category (e.g. "ExecutiveSafetyOverview").</summary>
    public string Code { get; }

    /// <summary>Human-readable display name matching the source catalog wording.</summary>
    public string DisplayName { get; }

    /// <summary>Primary owning role for this dashboard, per "4. Role-Based Dashboards".</summary>
    public DashboardRole PrimaryOwningRole { get; }

    private DashboardCategory(string code, string displayName, DashboardRole primaryOwningRole)
    {
        Code = code;
        DisplayName = displayName;
        PrimaryOwningRole = primaryOwningRole;
    }

    /// <summary>1. Executive Safety Overview Dashboard.</summary>
    public static readonly DashboardCategory ExecutiveSafetyOverview = new("ExecutiveSafetyOverview", "Executive Safety Overview Dashboard", DashboardRole.Executive);

    /// <summary>2. Risk Management Dashboard.</summary>
    public static readonly DashboardCategory RiskManagement = new("RiskManagement", "Risk Management Dashboard", DashboardRole.SafetyManager);

    /// <summary>3. Incident &amp; Near-Miss Dashboard.</summary>
    public static readonly DashboardCategory IncidentNearMiss = new("IncidentNearMiss", "Incident & Near-Miss Dashboard", DashboardRole.SafetyManager);

    /// <summary>4. PPE Compliance Dashboard.</summary>
    public static readonly DashboardCategory PpeCompliance = new("PpeCompliance", "PPE Compliance Dashboard", DashboardRole.SafetyManager);

    /// <summary>5. Machine Safety &amp; PM Dashboard.</summary>
    public static readonly DashboardCategory MachineSafetyPm = new("MachineSafetyPm", "Machine Safety & PM Dashboard", DashboardRole.SafetyManager);

    /// <summary>6. Observability Dashboard (AI + IoT).</summary>
    public static readonly DashboardCategory Observability = new("Observability", "Observability Dashboard (AI + IoT)", DashboardRole.MaintenanceEngineering);

    /// <summary>7. Permit-to-Work (PTW) Dashboard.</summary>
    public static readonly DashboardCategory PermitToWork = new("PermitToWork", "Permit-to-Work Dashboard", DashboardRole.SafetyManager);

    /// <summary>8. Scheduling &amp; Fatigue Dashboard.</summary>
    public static readonly DashboardCategory SchedulingFatigue = new("SchedulingFatigue", "Scheduling & Fatigue Dashboard", DashboardRole.Supervisor);

    /// <summary>9. Contractor Safety Dashboard.</summary>
    public static readonly DashboardCategory ContractorSafety = new("ContractorSafety", "Contractor Safety Dashboard", DashboardRole.ContractorCoordinator);

    /// <summary>10. Safety Training &amp; Certification Dashboard.</summary>
    public static readonly DashboardCategory TrainingCertification = new("TrainingCertification", "Safety Training & Certification Dashboard", DashboardRole.ContractorCoordinator);

    /// <summary>11. Alerts &amp; Emergency Response Dashboard.</summary>
    public static readonly DashboardCategory AlertsEmergencyResponse = new("AlertsEmergencyResponse", "Alerts & Emergency Response Dashboard", DashboardRole.SafetyManager);

    /// <summary>12. Safety Culture Dashboard.</summary>
    public static readonly DashboardCategory SafetyCulture = new("SafetyCulture", "Safety Culture Dashboard", DashboardRole.SafetyManager);

    /// <summary>13. Environmental &amp; Facility Dashboard.</summary>
    public static readonly DashboardCategory EnvironmentalFacility = new("EnvironmentalFacility", "Environmental & Facility Dashboard", DashboardRole.Supervisor);

    /// <summary>14. Material Handling &amp; Traffic Dashboard.</summary>
    public static readonly DashboardCategory MaterialHandlingTraffic = new("MaterialHandlingTraffic", "Material Handling & Traffic Dashboard", DashboardRole.Supervisor);

    /// <summary>15. Electrical Safety Dashboard.</summary>
    public static readonly DashboardCategory ElectricalSafety = new("ElectricalSafety", "Electrical Safety Dashboard", DashboardRole.MaintenanceEngineering);

    /// <summary>16. Checklists &amp; Audits Dashboard.</summary>
    public static readonly DashboardCategory ChecklistsAudits = new("ChecklistsAudits", "Checklists & Audits Dashboard", DashboardRole.Supervisor);

    /// <summary>17. Quality &amp; Process Safety Dashboard.</summary>
    public static readonly DashboardCategory QualityProcessSafety = new("QualityProcessSafety", "Quality & Process Safety Dashboard", DashboardRole.MaintenanceEngineering);

    /// <summary>18. Security &amp; Access Control Dashboard.</summary>
    public static readonly DashboardCategory SecurityAccessControl = new("SecurityAccessControl", "Security & Access Control Dashboard", DashboardRole.SafetyManager);

    /// <summary>19. Full Program Master Dashboard — top-level executive dashboard aggregating everything.</summary>
    public static readonly DashboardCategory FullProgramMaster = new("FullProgramMaster", "Full Program Master Dashboard", DashboardRole.Executive);

    /// <summary>All defined categories, in catalog order.</summary>
    public static readonly IReadOnlyList<DashboardCategory> All = new[]
    {
        ExecutiveSafetyOverview, RiskManagement, IncidentNearMiss, PpeCompliance, MachineSafetyPm,
        Observability, PermitToWork, SchedulingFatigue, ContractorSafety, TrainingCertification,
        AlertsEmergencyResponse, SafetyCulture, EnvironmentalFacility, MaterialHandlingTraffic,
        ElectricalSafety, ChecklistsAudits, QualityProcessSafety, SecurityAccessControl, FullProgramMaster
    };

    /// <summary>Default category assigned when none is specified.</summary>
    public static readonly DashboardCategory Default = ExecutiveSafetyOverview;

    /// <summary>Resolves a <see cref="DashboardCategory"/> from its canonical code. Throws when unknown.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="code"/> does not match a known category.</exception>
    public static DashboardCategory From(string code)
        => TryParse(code, out var category) ? category! : throw new ArgumentException($"Unknown DashboardCategory code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a <see cref="DashboardCategory"/> from its canonical code.</summary>
    public static bool TryParse(string? code, out DashboardCategory? category)
    {
        category = All.FirstOrDefault(c => string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));
        return category is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> matches a known category.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(DashboardCategory? other) => other is not null && Code == other.Code;
    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as DashboardCategory);
    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);
    /// <inheritdoc/>
    public override string ToString() => Code;
}
