// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentSeverity.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for OSHA-aligned injury severity classification. Grounded in the
//                  Safety Scorecards Module's lagging-indicator taxonomy (TRIR "recordable
//                  injuries", DART "Days Away, Restricted, or Transferred", "Lost Workdays") which
//                  the Accident Management module's "severity classification dashboards" feature
//                  reports against.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AccidentManagement.Enums;

/// <summary>Smart Enum representing OSHA-aligned injury severity classification for an Accident.</summary>
public sealed class AccidentSeverity : IEquatable<AccidentSeverity>
{
    /// <summary>Hazard/unsafe condition observed with no injury. Not OSHA-recordable.</summary>
    public static readonly AccidentSeverity NearMiss = new("NearMiss", "Near Miss", isOshaRecordable: false, isDartCase: false);

    /// <summary>Minor injury resolved with on-site first aid. Not OSHA-recordable.</summary>
    public static readonly AccidentSeverity FirstAidOnly = new("FirstAidOnly", "First Aid Only", isOshaRecordable: false, isDartCase: false);

    /// <summary>OSHA-recordable injury requiring medical treatment beyond first aid.</summary>
    public static readonly AccidentSeverity RecordableInjury = new("RecordableInjury", "Recordable Injury", isOshaRecordable: true, isDartCase: false);

    /// <summary>OSHA-recordable injury resulting in Days Away, Restricted duty, or job Transfer (DART case).</summary>
    public static readonly AccidentSeverity LostTimeInjury = new("LostTimeInjury", "Lost-Time / DART Injury", isOshaRecordable: true, isDartCase: true);

    /// <summary>Fatality. The most severe OSHA-recordable classification; triggers immediate regulatory reporting.</summary>
    public static readonly AccidentSeverity Fatality = new("Fatality", "Fatality", isOshaRecordable: true, isDartCase: true);

    /// <summary>All defined AccidentSeverity instances.</summary>
    public static readonly IReadOnlyList<AccidentSeverity> All = [NearMiss, FirstAidOnly, RecordableInjury, LostTimeInjury, Fatality];

    /// <summary>The default AccidentSeverity instance, used when no explicit severity is supplied.</summary>
    public static AccidentSeverity Default => FirstAidOnly;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Whether this severity level counts toward TRIR (OSHA-recordable).</summary>
    public bool IsOshaRecordable { get; }

    /// <summary>Whether this severity level counts toward the DART rate.</summary>
    public bool IsDartCase { get; }

    private AccidentSeverity(string code, string displayName, bool isOshaRecordable, bool isDartCase)
    {
        Code = code;
        DisplayName = displayName;
        IsOshaRecordable = isOshaRecordable;
        IsDartCase = isDartCase;
    }

    /// <summary>Resolves an AccidentSeverity instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static AccidentSeverity From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown AccidentSeverity code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve an AccidentSeverity instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out AccidentSeverity value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized AccidentSeverity code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(AccidentSeverity? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as AccidentSeverity);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
