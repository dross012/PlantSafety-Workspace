/*==========================================================================================================================
    AUTHOR:             PlantSafety Intelligence Architecture Group
    CREATED:            2026-06-14
    FILE:               IntelligenceConstants.cs
    DESCRIPTION:        Domain-wide constants for the Intelligence bounded context. All numeric thresholds
                        and length limits are defined here and referenced by value objects and aggregates.
                        Safety constants (MinimumConfidenceScore) may only be raised, never lowered. (Safety Rule 4.4)

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. Numeric safety thresholds may only increase.
==========================================================================================================================*/

namespace Intelligence.Domain.Shared.Constants;

/// <summary>
/// Domain-wide constants for the Intelligence bounded context.
/// </summary>
public static class IntelligenceConstants
{
    // ── Safety Rules ────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// SAFETY-CRITICAL: Minimum confidence score required for automated safety actions (Safety Rule 4.4).
    /// Predictions below this floor must not drive any automated decision. May only be raised, never lowered.
    /// </summary>
    public const double MinimumConfidenceScore = 0.70;

    // ── String Length Limits ─────────────────────────────────────────────────────────────────────────

    /// <summary>Maximum character length for a safety prediction subject string.</summary>
    public const int MaxPredictionSubjectLength = 500;

    /// <summary>Maximum character length for an investigation subject string.</summary>
    public const int MaxInvestigationSubjectLength = 500;

    /// <summary>Maximum character length for a model name.</summary>
    public const int MaxModelNameLength = 200;

    /// <summary>Maximum character length for a feature name.</summary>
    public const int MaxFeatureNameLength = 200;

    // ── Hash Lengths ─────────────────────────────────────────────────────────────────────────────────

    /// <summary>Required length of a SHA-256 dataset hash expressed as a lowercase hexadecimal string.</summary>
    public const int DatasetHashHexLength = 64;

    // ── Collection Limits ────────────────────────────────────────────────────────────────────────────

    /// <summary>Maximum number of findings allowed on a single investigation.</summary>
    public const int MaxFindingsPerInvestigation = 100;

    /// <summary>Maximum number of evidence items allowed on a single safety prediction.</summary>
    public const int MaxEvidencePerPrediction = 50;

    // ── Schema Versioning ────────────────────────────────────────────────────────────────────────────

    /// <summary>The current default schema version applied to new domain records.</summary>
    public const string DefaultSchemaVersion = "2026.1";
}
