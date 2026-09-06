// ============================================================================
//  Project:        Shared.EventBus.Abstractions
//  File:           HazardInferenceRules.cs
//  Created:        2026-08-08
//  Description:    Phase B.4 migration scaffolding (TEMPORARY). Deliberately
//                  duplicates the keyword-classification ALGORITHM already
//                  implemented in Shared.FacilityDomain.Abstractions.Hazards.
//                  HazardInferenceRules (Phase B.2) -- not the enum, which
//                  Shared.EventBus cannot reference (see EventEnvelopeV2.cs's
//                  header for why this library stays dependency-free of any
//                  specific bounded context). Returns the classified hazard
//                  type's NAME as a plain string, matching the member names
//                  of Shared.FacilityDomain.Abstractions.Hazards.HazardType
//                  exactly, so a consumer that DOES reference that enum can
//                  Enum.Parse it back losslessly.
//                  Delete this file (and consolidate on a single
//                  implementation) once a proper enrichment service can
//                  reference both Shared.EventBus and Shared.FacilityDomain
//                  in one place.
// ============================================================================
namespace Shared.EventBus.Abstractions.Contracts;

/// <summary>Deterministic, keyword-based hazard-type classification, returning the hazard type's name as a string.</summary>
public static class HazardInferenceRules
{
    /// <summary>Infers the hazard type name for an AccidentManagement event.</summary>
    public static string InferHazardTypeFromAccident(string accidentEventType, string? description = null) =>
        InferFromText(accidentEventType, description);

    /// <summary>Infers the hazard type name for a SafetyEvents event.</summary>
    public static string InferHazardTypeFromSafetyEvent(string safetyEventType, string? description = null) =>
        InferFromText(safetyEventType, description);

    /// <summary>Infers the hazard type name for a CrisisManagement event.</summary>
    public static string InferHazardTypeFromCrisisEvent(string crisisEventType, string? description = null) =>
        InferFromText(crisisEventType, description);

    /// <summary>Infers the hazard type name for an EmergencyManagement event.</summary>
    public static string InferHazardTypeFromEmergencyEvent(string emergencyEventType, string? description = null) =>
        InferFromText(emergencyEventType, description);

    private static string InferFromText(string eventType, string? description)
    {
        string haystack = $"{eventType} {description}".ToUpperInvariant();

        if (haystack.Contains("FIRE", StringComparison.Ordinal))
        {
            return "Fire";
        }

        if (haystack.Contains("CHEMICAL", StringComparison.Ordinal) || haystack.Contains("SPILL", StringComparison.Ordinal))
        {
            return "ChemicalSpill";
        }

        if (haystack.Contains("INJUR", StringComparison.Ordinal))
        {
            return "Injury";
        }

        if (haystack.Contains("ENVIRONMENT", StringComparison.Ordinal) || haystack.Contains("RELEASE", StringComparison.Ordinal) || haystack.Contains("EMISSION", StringComparison.Ordinal))
        {
            return "EnvironmentalRelease";
        }

        if (haystack.Contains("CONFINED SPACE", StringComparison.Ordinal) || haystack.Contains("CONFINEDSPACE", StringComparison.Ordinal))
        {
            return "ConfinedSpace";
        }

        if (haystack.Contains("LOCKOUT", StringComparison.Ordinal) || haystack.Contains("TAGOUT", StringComparison.Ordinal) || haystack.Contains("LOTO", StringComparison.Ordinal))
        {
            return "LOTOFailure";
        }

        if (haystack.Contains("HOT WORK", StringComparison.Ordinal) || haystack.Contains("HOTWORK", StringComparison.Ordinal) || haystack.Contains("WELDING", StringComparison.Ordinal))
        {
            return "HotWork";
        }

        if (haystack.Contains("FLEET", StringComparison.Ordinal) || haystack.Contains("VEHICLE", StringComparison.Ordinal) || haystack.Contains("COLLISION", StringComparison.Ordinal))
        {
            return "FleetIncident";
        }

        return "Other";
    }
}
