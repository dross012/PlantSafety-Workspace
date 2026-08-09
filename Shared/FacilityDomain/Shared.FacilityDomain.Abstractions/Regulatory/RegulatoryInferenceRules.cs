// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Regulatory
//  File:           RegulatoryInferenceRules.cs
//  Created:        2026-08-08
//  Description:    Phase C.4: deterministic inference of the applicable
//                  RegulatoryJurisdiction set from a HazardType and
//                  SeverityLevel. Mirrors HazardInferenceRules/
//                  SeverityInferenceRules' precedent of deterministic,
//                  rule-based (not ML) classification -- an event can map
//                  to multiple jurisdictions at once (e.g. a fire is both
//                  OSHA- and NFPA-relevant), so this returns a set, not a
//                  single value.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;

namespace Shared.FacilityDomain.Abstractions.Regulatory;

/// <summary>Deterministic inference of applicable <see cref="RegulatoryJurisdiction"/> values from hazard/severity context.</summary>
public static class RegulatoryInferenceRules
{
    /// <summary>
    /// Infers every regulatory jurisdiction applicable to an event with the given hazard type and severity.
    /// OSHA applies to every Moderate-or-worse event (workplace-safety baseline); the rest are hazard-specific.
    /// </summary>
    public static IReadOnlyList<RegulatoryJurisdiction> Infer(HazardType hazardType, SeverityLevel severityLevel)
    {
        var jurisdictions = new List<RegulatoryJurisdiction>();

        if (severityLevel >= SeverityLevel.Moderate)
        {
            jurisdictions.Add(RegulatoryJurisdiction.OSHA);
        }

        switch (hazardType)
        {
            case HazardType.Fire:
                jurisdictions.Add(RegulatoryJurisdiction.NFPA);
                break;
            case HazardType.ChemicalSpill:
            case HazardType.EnvironmentalRelease:
                jurisdictions.Add(RegulatoryJurisdiction.EPA);
                break;
            case HazardType.FleetIncident:
                jurisdictions.Add(RegulatoryJurisdiction.DOT);
                break;
        }

        if (severityLevel == SeverityLevel.Catastrophic)
        {
            jurisdictions.Add(RegulatoryJurisdiction.State);
            jurisdictions.Add(RegulatoryJurisdiction.Local);
        }

        return jurisdictions;
    }
}
