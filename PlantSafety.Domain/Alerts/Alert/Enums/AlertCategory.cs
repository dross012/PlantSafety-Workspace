// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Alerts.Alert
//  Layer:          Domain
//  File:           AlertCategory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the 11-category manufacturing alert taxonomy defined in
//                  "Comprehensive Manufacturing Safety Alerts" (PlantSafety_Program_Structure).
//                  Carries metadata (default severity, override-all flag, response instructions)
//                  because category drives workflow/escalation/delivery-channel selection —
//                  satisfying the Enums Rule (2.7) requirement for Smart Enums when a type has
//                  business rules and metadata.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Alerts.Alert.Enums;

/// <summary>
/// Classifies an <see cref="Aggregates.Alert"/> into one of the manufacturing alert catalog
/// categories. Each category carries a default severity and canonical response instructions.
/// </summary>
public sealed class AlertCategory : IEquatable<AlertCategory>
{
    /// <summary>Canonical string code for this category (e.g. "LifeSafetyEmergency").</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Default severity assigned to newly triggered alerts of this category.</summary>
    public AlertSeverity DefaultSeverity { get; }

    /// <summary>
    /// True when this category's alerts MUST override/interrupt all other active notifications
    /// (source doc: "These alerts require instant action and override all other notifications").
    /// </summary>
    public bool OverridesAllNotifications { get; }

    /// <summary>Canonical, comma-joined response instructions for this category.</summary>
    public string ResponseInstructions { get; }

    private AlertCategory(string code, string displayName, AlertSeverity defaultSeverity, bool overridesAllNotifications, string responseInstructions)
    {
        Code = code;
        DisplayName = displayName;
        DefaultSeverity = defaultSeverity;
        OverridesAllNotifications = overridesAllNotifications;
        ResponseInstructions = responseInstructions;
    }

    /// <summary>1. Life-Safety Emergency Alerts (fire, explosion, chemical spill, evacuation, active threat).</summary>
    public static readonly AlertCategory LifeSafetyEmergency = new(
        "LifeSafetyEmergency", "Life-Safety Emergency Alerts", AlertSeverity.Critical, true,
        "Evacuate; Shelter in place; Avoid area; Activate emergency response team");

    /// <summary>2. Injury & "Man Down" Alerts (worker collapse, panic button, lone-worker distress, fall detection).</summary>
    public static readonly AlertCategory InjuryManDown = new(
        "InjuryManDown", "Injury & Man Down Alerts", AlertSeverity.Critical, true,
        "Dispatch first responders; Provide exact GPS or indoor location; Notify supervisors and safety officers");

    /// <summary>3. Operational & Hazard Alerts (E-Stop, guarding failure, interlock bypass, LOTO violation).</summary>
    public static readonly AlertCategory OperationalHazard = new(
        "OperationalHazard", "Operational & Hazard Alerts", AlertSeverity.High, false,
        "Stop work; Lockout equipment; Notify maintenance; Isolate hazard zone");

    /// <summary>4. Environmental Condition Alerts (air quality, temperature, humidity, noise, water leaks).</summary>
    public static readonly AlertCategory EnvironmentalCondition = new(
        "EnvironmentalCondition", "Environmental Condition Alerts", AlertSeverity.Medium, false,
        "Ventilate area; Reduce exposure; Shut down equipment; Evacuate if thresholds exceed safe limits");

    /// <summary>5. Facility & Traffic Control Alerts (forklift proximity, dock congestion, blocked exits).</summary>
    public static readonly AlertCategory FacilityTrafficControl = new(
        "FacilityTrafficControl", "Facility & Traffic Control Alerts", AlertSeverity.Medium, false,
        "Clear area; Slow down equipment; Redirect traffic; Remove obstructions");

    /// <summary>6. Behavioral & PPE Compliance Alerts (missing/incorrect PPE, unsafe behavior, restricted zone entry).</summary>
    public static readonly AlertCategory BehavioralPpeCompliance = new(
        "BehavioralPpeCompliance", "Behavioral & PPE Compliance Alerts", AlertSeverity.Medium, false,
        "Correct PPE; Stop unsafe behavior; Notify supervisor; Log behavioral observation");

    /// <summary>7. Permit-to-Work (PTW) Alerts (permit expiration, unauthorized work, missing controls).</summary>
    public static readonly AlertCategory PermitToWork = new(
        "PermitToWork", "Permit-to-Work Alerts", AlertSeverity.High, false,
        "Stop work; Verify permit; Apply required controls; Notify safety officer");

    /// <summary>8. Maintenance & Equipment Health Alerts (PM overdue, sensor failure, motor overload).</summary>
    public static readonly AlertCategory MaintenanceEquipmentHealth = new(
        "MaintenanceEquipmentHealth", "Maintenance & Equipment Health Alerts", AlertSeverity.Medium, false,
        "Schedule PM; Shut down equipment; Inspect components; Notify maintenance");

    /// <summary>9. Security & Access Control Alerts (unauthorized badge access, tailgating, perimeter breach).</summary>
    public static readonly AlertCategory SecurityAccessControl = new(
        "SecurityAccessControl", "Security & Access Control Alerts", AlertSeverity.High, false,
        "Notify security; Lock down zone; Investigate breach");

    /// <summary>10. Quality & Process Deviation Alerts (out-of-spec parameters, batch contamination).</summary>
    public static readonly AlertCategory QualityProcessDeviation = new(
        "QualityProcessDeviation", "Quality & Process Deviation Alerts", AlertSeverity.Medium, false,
        "Stop production; Quarantine batch; Notify quality team");

    /// <summary>11. Compliance & Audit Alerts (training expiration, certification lapse, audit overdue).</summary>
    public static readonly AlertCategory ComplianceAudit = new(
        "ComplianceAudit", "Compliance & Audit Alerts", AlertSeverity.Low, false,
        "Complete training; Schedule audit; Update documentation");

    /// <summary>All defined categories, in catalog order.</summary>
    public static readonly IReadOnlyList<AlertCategory> All = new[]
    {
        LifeSafetyEmergency, InjuryManDown, OperationalHazard, EnvironmentalCondition,
        FacilityTrafficControl, BehavioralPpeCompliance, PermitToWork, MaintenanceEquipmentHealth,
        SecurityAccessControl, QualityProcessDeviation, ComplianceAudit
    };

    /// <summary>Default category assigned when none is specified.</summary>
    public static readonly AlertCategory Default = OperationalHazard;

    /// <summary>Resolves an <see cref="AlertCategory"/> from its canonical code. Throws when unknown.</summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="code"/> does not match a known category.</exception>
    public static AlertCategory From(string code)
        => TryParse(code, out var category)
            ? category!
            : throw new ArgumentException($"Unknown AlertCategory code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve an <see cref="AlertCategory"/> from its canonical code.</summary>
    public static bool TryParse(string? code, out AlertCategory? category)
    {
        category = All.FirstOrDefault(c => string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));
        return category is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> matches a known category.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(AlertCategory? other) => other is not null && Code == other.Code;
    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as AlertCategory);
    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);
    /// <inheritdoc/>
    public override string ToString() => Code;
}
