// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskScore.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Value object for the Severity x Likelihood risk-matrix cell, taken verbatim from
//                  Full Risk Management Process PHASE 1 Step 2 ("Measure & Analyze Risk"):
//                  "Severity: 1-5, Likelihood: 1-5, Risk Score = Severity x Likelihood, Risk
//                  Level: Low, Medium, High, Critical" and confirmed by Appendix B -- Safety
//                  Metrics & Formulas ("Risk Score = Severity x Likelihood").
//                  The document does not specify exact numeric Score-to-RiskLevel band boundaries;
//                  this implementation uses the industry-standard 5x5 risk-matrix banding
//                  (1-4 Low, 5-9 Medium, 10-14 High, 15-25 Critical), noted here as an
//                  assumption made to fill a documentation gap.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.RiskManagement.Enums;

namespace PlantSafety.Domain.RiskManagement.ValueObjects;

/// <summary>Immutable value object representing a Severity x Likelihood risk-matrix cell.</summary>
public readonly record struct RiskScore
{
    /// <summary>Severity rating, 1 (negligible) through 5 (catastrophic).</summary>
    public int Severity { get; }

    /// <summary>Likelihood rating, 1 (rare) through 5 (almost certain).</summary>
    public int Likelihood { get; }

    /// <summary>Computed Risk Score = Severity x Likelihood (Appendix B).</summary>
    public int Score => Severity * Likelihood;

    /// <summary>Creates a validated RiskScore from a 1-5 severity and 1-5 likelihood rating.</summary>
    public RiskScore(int severity, int likelihood)
    {
        if (severity is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(severity), severity, "Severity must be between 1 and 5.");
        if (likelihood is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(likelihood), likelihood, "Likelihood must be between 1 and 5.");

        Severity = severity;
        Likelihood = likelihood;
    }

    /// <summary>
    /// Derives the qualitative RiskLevel banding for this score using the standard 5x5 risk-matrix
    /// convention: 1-4 Low, 5-9 Medium, 10-14 High, 15-25 Critical.
    /// </summary>
    public RiskLevel ToRiskLevel() => Score switch
    {
        <= 4 => RiskLevel.Low,
        <= 9 => RiskLevel.Medium,
        <= 14 => RiskLevel.High,
        _ => RiskLevel.Critical,
    };

    /// <inheritdoc/>
    public override string ToString() => $"{Severity}x{Likelihood}={Score} ({ToRiskLevel().DisplayName})";
}
