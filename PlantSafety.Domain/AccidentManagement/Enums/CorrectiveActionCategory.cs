// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           CorrectiveActionCategory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for corrective action classification, taken verbatim from
//                  Appendix G -- Corrective Action Categories of PlantSafety_Program_Structure_extract.txt
//                  ("Used in Incident Management, Auditing, and Scorecards").
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AccidentManagement.Enums;

/// <summary>Smart Enum representing the category of a CAPA corrective action (Appendix G).</summary>
public sealed class CorrectiveActionCategory : IEquatable<CorrectiveActionCategory>
{
    /// <summary>Engineering Controls -- guards, ventilation, automation, physical redesign.</summary>
    public static readonly CorrectiveActionCategory EngineeringControls = new("EngineeringControls", "Engineering Controls");

    /// <summary>Administrative Controls -- policies, signage, scheduling changes.</summary>
    public static readonly CorrectiveActionCategory AdministrativeControls = new("AdministrativeControls", "Administrative Controls");

    /// <summary>PPE Controls -- new or upgraded personal protective equipment requirements.</summary>
    public static readonly CorrectiveActionCategory PpeControls = new("PpeControls", "PPE Controls");

    /// <summary>Training -- new or refresher training assigned to close a competency gap.</summary>
    public static readonly CorrectiveActionCategory Training = new("Training", "Training");

    /// <summary>Maintenance -- repair, replacement, or preventive maintenance of equipment.</summary>
    public static readonly CorrectiveActionCategory Maintenance = new("Maintenance", "Maintenance");

    /// <summary>Procedural Updates -- revisions to SOPs, JSAs, or work instructions.</summary>
    public static readonly CorrectiveActionCategory ProceduralUpdates = new("ProceduralUpdates", "Procedural Updates");

    /// <summary>Environmental Controls -- ventilation, spill containment, air quality mitigation.</summary>
    public static readonly CorrectiveActionCategory EnvironmentalControls = new("EnvironmentalControls", "Environmental Controls");

    /// <summary>All defined CorrectiveActionCategory instances (Appendix G).</summary>
    public static readonly IReadOnlyList<CorrectiveActionCategory> All =
        [EngineeringControls, AdministrativeControls, PpeControls, Training, Maintenance, ProceduralUpdates, EnvironmentalControls];

    /// <summary>The default CorrectiveActionCategory instance, used when no explicit category is supplied.</summary>
    public static CorrectiveActionCategory Default => AdministrativeControls;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    private CorrectiveActionCategory(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Resolves a CorrectiveActionCategory instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static CorrectiveActionCategory From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown CorrectiveActionCategory code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a CorrectiveActionCategory instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out CorrectiveActionCategory value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized CorrectiveActionCategory code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(CorrectiveActionCategory? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as CorrectiveActionCategory);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
