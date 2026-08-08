// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.ComplianceRisk
//  Layer:          Abstractions
//  File:           RiskTrend.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic comparison between two RiskScore snapshots
//                  for the same facility (same shape/role as
//                  ComplianceTrend.cs, but for RiskScore.OverallRiskLevel).
//                  RiskLevel has no numeric type of its own, so Delta is
//                  computed over the enum's ordinal position
//                  (Low=0/Medium=1/High=2/Critical=3) -- a negative Delta
//                  means risk decreased (improved), a positive Delta means
//                  risk increased (worsened). Stateless, same as
//                  ComplianceTrend.cs -- the caller supplies both snapshots.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;

/// <summary>Which direction a facility's overall risk level moved between two snapshots.</summary>
public enum RiskTrendDirection
{
    /// <summary>OverallRiskLevel decreased (risk went down).</summary>
    Improving,

    /// <summary>OverallRiskLevel is unchanged.</summary>
    Stable,

    /// <summary>OverallRiskLevel increased (risk went up).</summary>
    Worsening,
}

/// <summary>
/// A deterministic comparison between a facility's previous and current <see cref="RiskScore"/>.
/// </summary>
/// <param name="FacilityId">The facility this trend was computed for.</param>
/// <param name="LastLevel">The previous snapshot's <see cref="RiskScore.OverallRiskLevel"/>.</param>
/// <param name="CurrentLevel">The current snapshot's <see cref="RiskScore.OverallRiskLevel"/>.</param>
/// <param name="Delta">The ordinal difference (CurrentLevel - LastLevel); negative means risk decreased.</param>
/// <param name="Direction">Improving if Delta &lt; 0, Worsening if Delta &gt; 0, otherwise Stable.</param>
public sealed record RiskTrend(
    Guid FacilityId,
    RiskLevel LastLevel,
    RiskLevel CurrentLevel,
    int Delta,
    RiskTrendDirection Direction);

/// <summary>Builds <see cref="RiskTrend"/> instances deterministically from two <see cref="RiskScore"/> snapshots.</summary>
public static class RiskTrendBuilder
{
    /// <summary>Builds the trend between two risk score snapshots for the same facility.</summary>
    /// <exception cref="ArgumentException"><paramref name="previous"/> and <paramref name="current"/> have different <see cref="RiskScore.FacilityId"/> values.</exception>
    public static RiskTrend Build(RiskScore previous, RiskScore current)
    {
        if (previous.FacilityId != current.FacilityId)
        {
            throw new ArgumentException("previous and current must be scores for the same facility.", nameof(current));
        }

        int delta = (int)current.OverallRiskLevel - (int)previous.OverallRiskLevel;

        RiskTrendDirection direction = delta switch
        {
            < 0 => RiskTrendDirection.Improving,
            > 0 => RiskTrendDirection.Worsening,
            _ => RiskTrendDirection.Stable,
        };

        return new RiskTrend(current.FacilityId, previous.OverallRiskLevel, current.OverallRiskLevel, delta, direction);
    }
}
