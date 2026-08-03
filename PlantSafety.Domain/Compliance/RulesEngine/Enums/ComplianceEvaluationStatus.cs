// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceEvaluationStatus.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the lifecycle status of a ComplianceEvaluation aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RulesEngine.Enums;

/// <summary>Smart Enum representing the lifecycle status of a ComplianceEvaluation.</summary>
public sealed class ComplianceEvaluationStatus : IEquatable<ComplianceEvaluationStatus>
{
    /// <summary>Evaluation has been started but the rule pipeline has not yet completed.</summary>
    public static readonly ComplianceEvaluationStatus InProgress = new("InProgress", "In Progress");

    /// <summary>Evaluation completed with no failing rule results.</summary>
    public static readonly ComplianceEvaluationStatus Compliant = new("Compliant", "Compliant");

    /// <summary>Evaluation completed with one or more failing rule results.</summary>
    public static readonly ComplianceEvaluationStatus NonCompliant = new("NonCompliant", "Non-Compliant");

    /// <summary>All defined ComplianceEvaluationStatus instances.</summary>
    public static readonly IReadOnlyList<ComplianceEvaluationStatus> All = [InProgress, Compliant, NonCompliant];

    /// <summary>The default ComplianceEvaluationStatus instance.</summary>
    public static ComplianceEvaluationStatus Default => InProgress;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    private ComplianceEvaluationStatus(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Resolves a ComplianceEvaluationStatus instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static ComplianceEvaluationStatus From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown ComplianceEvaluationStatus code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a ComplianceEvaluationStatus instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out ComplianceEvaluationStatus value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(ComplianceEvaluationStatus? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ComplianceEvaluationStatus);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
