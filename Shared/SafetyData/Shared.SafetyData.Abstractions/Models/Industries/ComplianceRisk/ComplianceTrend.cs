// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.ComplianceRisk
//  Layer:          Abstractions
//  File:           ComplianceTrend.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic comparison between two ComplianceScore
//                  snapshots for the same facility, taken at two different
//                  points in time. This subsystem is stateless -- it never
//                  stores or looks up history itself; the caller supplies
//                  both the previous and current ComplianceScore (each
//                  itself built by ComplianceScoreBuilder.Build against a
//                  FacilityProfile snapshot taken at that point in time).
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;

/// <summary>Which direction a facility's compliance score moved between two snapshots.</summary>
public enum ComplianceTrendDirection
{
    /// <summary>PercentComplete increased.</summary>
    Improving,

    /// <summary>PercentComplete is unchanged.</summary>
    Stable,

    /// <summary>PercentComplete decreased.</summary>
    Declining,
}

/// <summary>
/// A deterministic comparison between a facility's previous and current <see cref="ComplianceScore"/>.
/// </summary>
/// <param name="FacilityId">The facility this trend was computed for.</param>
/// <param name="LastScore">The previous snapshot's <see cref="ComplianceScore.PercentComplete"/>.</param>
/// <param name="CurrentScore">The current snapshot's <see cref="ComplianceScore.PercentComplete"/>.</param>
/// <param name="Delta">CurrentScore - LastScore.</param>
/// <param name="Direction">Improving if Delta &gt; 0, Declining if Delta &lt; 0, otherwise Stable.</param>
public sealed record ComplianceTrend(
    Guid FacilityId,
    double LastScore,
    double CurrentScore,
    double Delta,
    ComplianceTrendDirection Direction);

/// <summary>Builds <see cref="ComplianceTrend"/> instances deterministically from two <see cref="ComplianceScore"/> snapshots.</summary>
public static class ComplianceTrendBuilder
{
    /// <summary>Builds the trend between two compliance score snapshots for the same facility.</summary>
    /// <exception cref="ArgumentException"><paramref name="previous"/> and <paramref name="current"/> have different <see cref="ComplianceScore.FacilityId"/> values.</exception>
    public static ComplianceTrend Build(ComplianceScore previous, ComplianceScore current)
    {
        if (previous.FacilityId != current.FacilityId)
        {
            throw new ArgumentException("previous and current must be scores for the same facility.", nameof(current));
        }

        double delta = current.PercentComplete - previous.PercentComplete;

        ComplianceTrendDirection direction = delta switch
        {
            > 0d => ComplianceTrendDirection.Improving,
            < 0d => ComplianceTrendDirection.Declining,
            _ => ComplianceTrendDirection.Stable,
        };

        return new ComplianceTrend(current.FacilityId, previous.PercentComplete, current.PercentComplete, delta, direction);
    }
}
