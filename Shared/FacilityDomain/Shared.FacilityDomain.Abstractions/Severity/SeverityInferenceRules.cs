// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Severity
//  File:           SeverityInferenceRules.cs
//  Created:        2026-08-08
//  Description:    Phase C.3: deterministic severity inference from hazard
//                  type, employee injury count, machine failure count, and
//                  event-chain length -- the four inputs Path C's spec
//                  calls out. Mirrors HazardInferenceRules' precedent of
//                  deterministic, rule-based (not ML) classification with
//                  an explicit worst-case combination, never a guess
//                  dressed up as a fact.
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.Severity;

/// <summary>Deterministic inference of <see cref="SeverityLevel"/> from hazard, injury, failure, and event-chain context.</summary>
public static class SeverityInferenceRules
{
    /// <summary>
    /// Infers a severity level from the contributing factors of a safety event. Each factor is scored
    /// independently and the highest (worst-case) resulting level wins -- severity never under-reports.
    /// </summary>
    public static SeverityLevel Infer(Hazards.HazardType hazardType, int employeeInjuryCount, int machineFailureCount, int eventChainLength)
    {
        SeverityLevel fromHazard = InferFromHazardType(hazardType);
        SeverityLevel fromInjury = InferFromInjuryCount(employeeInjuryCount);
        SeverityLevel fromFailure = InferFromMachineFailureCount(machineFailureCount);
        SeverityLevel fromChain = InferFromEventChainLength(eventChainLength);

        return Max(Max(fromHazard, fromInjury), Max(fromFailure, fromChain));
    }

    /// <summary>Infers a baseline severity from the hazard type alone -- fire/chemical/environmental hazards start higher.</summary>
    public static SeverityLevel InferFromHazardType(Hazards.HazardType hazardType) => hazardType switch
    {
        Hazards.HazardType.Fire => SeverityLevel.Severe,
        Hazards.HazardType.ChemicalSpill => SeverityLevel.Severe,
        Hazards.HazardType.EnvironmentalRelease => SeverityLevel.Moderate,
        Hazards.HazardType.Injury => SeverityLevel.Moderate,
        Hazards.HazardType.ConfinedSpace => SeverityLevel.Severe,
        Hazards.HazardType.LOTOFailure => SeverityLevel.Severe,
        Hazards.HazardType.HotWork => SeverityLevel.Moderate,
        Hazards.HazardType.FleetIncident => SeverityLevel.Moderate,
        Hazards.HazardType.Other => SeverityLevel.Minor,
        _ => SeverityLevel.Minor,
    };

    /// <summary>Infers severity from a cumulative employee injury count: 0 = Minor, 1 = Moderate, 2 = Severe, 3+ = Catastrophic.</summary>
    public static SeverityLevel InferFromInjuryCount(int employeeInjuryCount) => employeeInjuryCount switch
    {
        <= 0 => SeverityLevel.Minor,
        1 => SeverityLevel.Moderate,
        2 => SeverityLevel.Severe,
        _ => SeverityLevel.Catastrophic,
    };

    /// <summary>Infers severity from a cumulative machine failure count: 0 = Minor, 1 = Moderate, 2 = Severe, 3+ = Catastrophic.</summary>
    public static SeverityLevel InferFromMachineFailureCount(int machineFailureCount) => machineFailureCount switch
    {
        <= 0 => SeverityLevel.Minor,
        1 => SeverityLevel.Moderate,
        2 => SeverityLevel.Severe,
        _ => SeverityLevel.Catastrophic,
    };

    /// <summary>Infers severity from the length of a multi-hazard event chain: longer chains indicate cascading, worsening consequences.</summary>
    public static SeverityLevel InferFromEventChainLength(int eventChainLength) => eventChainLength switch
    {
        <= 1 => SeverityLevel.Minor,
        2 => SeverityLevel.Moderate,
        3 => SeverityLevel.Severe,
        _ => SeverityLevel.Catastrophic,
    };

    private static SeverityLevel Max(SeverityLevel left, SeverityLevel right) => left >= right ? left : right;
}
