// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IncidentReportabilityRules.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Computes whether a hazmat incident is reportable to PHMSA under 49 CFR
//                  171.15/171.16 (DOT Form 5800.1 telephonic/written reporting triggers).
//                  ILLUSTRATIVE SIMPLIFICATION -- see remarks below. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Enums;

namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.Rules;

/// <summary>
/// Determines whether a hazmat incident meets the PHMSA reporting threshold under
/// 49 CFR 171.15 (immediate/telephonic report) and 171.16 (detailed written report,
/// DOT Form 5800.1).
/// </summary>
/// <remarks>
/// THIS IS AN ILLUSTRATIVE SIMPLIFICATION, not a verbatim restatement of the regulation.
/// The real 49 CFR 171.15/171.16 reportability criteria are considerably more granular
/// (e.g. specific hazard-class thresholds, "major transportation artery" disruption,
/// evacuation triggers, marine pollutant release quantities, radioactive material
/// criteria, and a fatality/hospitalization distinction that this rule cannot make
/// because "requires hospitalization" is not knowable from the data captured on the
/// aggregate). Any production use of this rule MUST be validated against the current
/// text of 49 CFR 171.15/171.16 by regulatory/compliance counsel before being relied
/// upon for actual DOT 5800.1 filing decisions.
/// </remarks>
public static class IncidentReportabilityRules
{
    /// <summary>
    /// Illustrative property-damage threshold (USD) above which an incident is treated as
    /// reportable by this simplification. NOT the exact real dollar figure from 49 CFR
    /// 171.16 -- source the current threshold from the regulation text before relying on
    /// this value for actual filings.
    /// </summary>
    public const decimal IllustrativePropertyDamageThresholdUsd = 50_000m;

    /// <summary>
    /// Returns <see langword="true"/> when the incident meets this illustrative PHMSA
    /// reportability threshold.
    /// </summary>
    /// <remarks>
    /// NOTE: A bare <see cref="Enums.IncidentType.Spill"/> with no fatalities, no injuries,
    /// and no property damage over the illustrative threshold is deliberately NOT
    /// automatically reportable -- small-quantity spills are common in hazmat operations
    /// and not every spill triggers PHMSA reporting. Callers must not assume otherwise.
    /// </remarks>
    public static bool IsReportableToPhmsa(
        IncidentType type,
        int injuries,
        bool fatalitiesInvolved,
        decimal? propertyDamageEstimateUsd)
    {
        if (fatalitiesInvolved)
            return true;

        // "Requires hospitalization" cannot be determined from the data captured on this
        // aggregate, so as a conservative illustrative simplification any recorded injury
        // count greater than zero is treated as reportable.
        if (injuries > 0)
            return true;

        if (type == IncidentType.FireOrExplosion)
            return true;

        if (propertyDamageEstimateUsd is decimal damage && damage >= IllustrativePropertyDamageThresholdUsd)
            return true;

        return false;
    }
}
