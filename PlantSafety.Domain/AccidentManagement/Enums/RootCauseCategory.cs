// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           RootCauseCategory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for root-cause hazard classification, taken verbatim from
//                  Appendix F -- Hazard Categories of PlantSafety_Program_Structure_extract.txt
//                  ("Standard hazard classification used across Risk Management, Observability,
//                  and Auditing"). Reused here as the Accident Management RCA root-cause taxonomy.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AccidentManagement.Enums;

/// <summary>Smart Enum representing the root-cause hazard category identified by an investigation's RCA.</summary>
public sealed class RootCauseCategory : IEquatable<RootCauseCategory>
{
    /// <summary>Mechanical hazard (moving parts, pinch points, crush zones).</summary>
    public static readonly RootCauseCategory Mechanical = new("Mechanical", "Mechanical");

    /// <summary>Electrical hazard (arc flash, exposed conductors, faulty wiring).</summary>
    public static readonly RootCauseCategory Electrical = new("Electrical", "Electrical");

    /// <summary>Chemical hazard (toxins, corrosives, flammables).</summary>
    public static readonly RootCauseCategory Chemical = new("Chemical", "Chemical");

    /// <summary>Thermal hazard (burns, extreme temperature exposure).</summary>
    public static readonly RootCauseCategory Thermal = new("Thermal", "Thermal");

    /// <summary>Ergonomic hazard (lifting, repetitive motion, awkward posture).</summary>
    public static readonly RootCauseCategory Ergonomic = new("Ergonomic", "Ergonomic");

    /// <summary>Environmental hazard (noise, vibration, air quality, temperature).</summary>
    public static readonly RootCauseCategory Environmental = new("Environmental", "Environmental");

    /// <summary>Biological hazard.</summary>
    public static readonly RootCauseCategory Biological = new("Biological", "Biological");

    /// <summary>Process safety hazard (pressure, temperature, stored energy).</summary>
    public static readonly RootCauseCategory ProcessSafety = new("ProcessSafety", "Process Safety");

    /// <summary>Behavioral/human-factors root cause (fatigue, training gaps, at-risk behavior).</summary>
    public static readonly RootCauseCategory Behavioral = new("Behavioral", "Behavioral");

    /// <summary>All defined RootCauseCategory instances (Appendix F -- Hazard Categories).</summary>
    public static readonly IReadOnlyList<RootCauseCategory> All =
        [Mechanical, Electrical, Chemical, Thermal, Ergonomic, Environmental, Biological, ProcessSafety, Behavioral];

    /// <summary>The default RootCauseCategory instance, used when no explicit category is supplied.</summary>
    public static RootCauseCategory Default => Behavioral;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    private RootCauseCategory(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Resolves a RootCauseCategory instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static RootCauseCategory From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown RootCauseCategory code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a RootCauseCategory instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out RootCauseCategory value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized RootCauseCategory code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(RootCauseCategory? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as RootCauseCategory);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
