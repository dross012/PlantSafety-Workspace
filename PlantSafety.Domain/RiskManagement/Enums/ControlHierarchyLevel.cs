// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           ControlHierarchyLevel.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the Hierarchy of Controls, taken verbatim from Risk Management
//                  Module Section 8 and Full Risk Management Process PHASE 2 Step 5:
//                  "Controls must be applied in order of effectiveness: Elimination, Substitution,
//                  Engineering Controls, Administrative Controls, PPE." The system "can enforce
//                  this hierarchy by requiring justification when lower-level controls are used."
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.RiskManagement.Enums;

/// <summary>Smart Enum representing a rung of the Hierarchy of Controls, ordered by effectiveness (1 = most effective).</summary>
public sealed class ControlHierarchyLevel : IEquatable<ControlHierarchyLevel>
{
    /// <summary>Elimination -- remove the hazard entirely. Most effective control.</summary>
    public static readonly ControlHierarchyLevel Elimination = new("Elimination", "Elimination", 1);

    /// <summary>Substitution -- replace hazardous materials or processes.</summary>
    public static readonly ControlHierarchyLevel Substitution = new("Substitution", "Substitution", 2);

    /// <summary>Engineering Controls -- guards, interlocks, ventilation, automation.</summary>
    public static readonly ControlHierarchyLevel EngineeringControls = new("EngineeringControls", "Engineering Controls", 3);

    /// <summary>Administrative Controls -- policies, signage, scheduling, training.</summary>
    public static readonly ControlHierarchyLevel AdministrativeControls = new("AdministrativeControls", "Administrative Controls", 4);

    /// <summary>PPE -- the final protective barrier. Least effective control; always requires justification.</summary>
    public static readonly ControlHierarchyLevel Ppe = new("Ppe", "PPE", 5);

    /// <summary>All defined ControlHierarchyLevel instances, ordered by effectiveness rank.</summary>
    public static readonly IReadOnlyList<ControlHierarchyLevel> All = [Elimination, Substitution, EngineeringControls, AdministrativeControls, Ppe];

    /// <summary>The default ControlHierarchyLevel instance.</summary>
    public static ControlHierarchyLevel Default => EngineeringControls;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Effectiveness rank; 1 is most effective (Elimination), 5 is least effective (PPE).</summary>
    public int EffectivenessRank { get; }

    /// <summary>
    /// Whether selecting this control level requires documented justification, per the rule that
    /// using a lower-effectiveness control than Elimination/Substitution must be justified.
    /// </summary>
    public bool RequiresJustification => EffectivenessRank >= EngineeringControls.EffectivenessRank;

    private ControlHierarchyLevel(string code, string displayName, int effectivenessRank)
    {
        Code = code;
        DisplayName = displayName;
        EffectivenessRank = effectivenessRank;
    }

    /// <summary>Resolves a ControlHierarchyLevel instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static ControlHierarchyLevel From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown ControlHierarchyLevel code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a ControlHierarchyLevel instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out ControlHierarchyLevel value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized ControlHierarchyLevel code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(ControlHierarchyLevel? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ControlHierarchyLevel);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
