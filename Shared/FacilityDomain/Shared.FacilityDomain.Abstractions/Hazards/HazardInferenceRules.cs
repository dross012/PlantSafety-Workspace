// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Hazards
//  File:           HazardInferenceRules.cs
//  Created:        2026-08-08
//  Description:    Phase B.2: deterministic, keyword-based HazardType
//                  classification (mirrors the precedent set by Shared.
//                  SafetyData's OrchestrationEventCategoryCatalog.ForEventType,
//                  which classifies by EventType suffix). Operates on
//                  caller-supplied event-type/description strings rather
//                  than real AccidentManagement/SafetyEvents/CrisisManagement/
//                  EmergencyManagement domain types, since this bounded
//                  context has no dependency on those real domains and
//                  Phase B.3 (wiring these rules into each domain's actual
//                  event publishing) has not been authorized yet -- see
//                  this repository's own session notes on Path A vs. Path B.
//                  Until B.3 supplies real per-event free-text data, every
//                  input without a matching keyword classifies as
//                  HazardType.Other -- never a guess dressed up as a fact.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.Hazards;

/// <summary>Deterministic, keyword-based inference of <see cref="HazardType"/> from an event's type/description text.</summary>
public static class HazardInferenceRules
{
    /// <summary>Infers the hazard type for an AccidentManagement event.</summary>
    public static HazardType InferHazardTypeFromAccident(string accidentEventType, string? description = null) =>
        InferFromText(accidentEventType, description);

    /// <summary>Infers the hazard type for a SafetyEvents event.</summary>
    public static HazardType InferHazardTypeFromSafetyEvent(string safetyEventType, string? description = null) =>
        InferFromText(safetyEventType, description);

    /// <summary>Infers the hazard type for a CrisisManagement event.</summary>
    public static HazardType InferHazardTypeFromCrisisEvent(string crisisEventType, string? description = null) =>
        InferFromText(crisisEventType, description);

    /// <summary>Infers the hazard type for an EmergencyManagement event.</summary>
    public static HazardType InferHazardTypeFromEmergencyEvent(string emergencyEventType, string? description = null) =>
        InferFromText(emergencyEventType, description);

    private static HazardType InferFromText(string eventType, string? description)
    {
        string haystack = $"{eventType} {description}".ToUpperInvariant();

        if (haystack.Contains("FIRE", StringComparison.Ordinal))
        {
            return HazardType.Fire;
        }

        if (haystack.Contains("CHEMICAL", StringComparison.Ordinal) || haystack.Contains("SPILL", StringComparison.Ordinal))
        {
            return HazardType.ChemicalSpill;
        }

        if (haystack.Contains("INJUR", StringComparison.Ordinal))
        {
            return HazardType.Injury;
        }

        if (haystack.Contains("ENVIRONMENT", StringComparison.Ordinal) || haystack.Contains("RELEASE", StringComparison.Ordinal) || haystack.Contains("EMISSION", StringComparison.Ordinal))
        {
            return HazardType.EnvironmentalRelease;
        }

        if (haystack.Contains("CONFINED SPACE", StringComparison.Ordinal) || haystack.Contains("CONFINEDSPACE", StringComparison.Ordinal))
        {
            return HazardType.ConfinedSpace;
        }

        if (haystack.Contains("LOCKOUT", StringComparison.Ordinal) || haystack.Contains("TAGOUT", StringComparison.Ordinal) || haystack.Contains("LOTO", StringComparison.Ordinal))
        {
            return HazardType.LOTOFailure;
        }

        if (haystack.Contains("HOT WORK", StringComparison.Ordinal) || haystack.Contains("HOTWORK", StringComparison.Ordinal) || haystack.Contains("WELDING", StringComparison.Ordinal))
        {
            return HazardType.HotWork;
        }

        if (haystack.Contains("FLEET", StringComparison.Ordinal) || haystack.Contains("VEHICLE", StringComparison.Ordinal) || haystack.Contains("COLLISION", StringComparison.Ordinal))
        {
            return HazardType.FleetIncident;
        }

        return HazardType.Other;
    }
}
