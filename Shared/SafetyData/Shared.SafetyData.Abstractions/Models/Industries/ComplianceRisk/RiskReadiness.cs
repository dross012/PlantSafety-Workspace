// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.ComplianceRisk
//  Layer:          Abstractions
//  File:           RiskReadiness.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Readiness indicators for a facility's hazard response,
//                  emergency response, and event routing -- computed
//                  strictly from HazardMapping.For(industry) (hazard
//                  coverage), FacilityEventRoutingRulesBuilder.Build
//                  (event routing coverage), and the facility's own
//                  FacilityProfile.ComplianceProfile.MissingElements. No
//                  independent recomputation of hazards, routes, or
//                  compliance state. Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Consumers;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

namespace Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;

/// <summary>
/// A single facility's readiness across hazard response, emergency response, and event routing.
/// </summary>
/// <param name="FacilityId">The facility this readiness was computed for.</param>
/// <param name="HazardResponseReady">True if every hazard category the facility's industry carries (<see cref="HazardMapping.For"/>) has a completed (non-missing) SOP.</param>
/// <param name="EmergencyResponseReady">True if no required <see cref="Facilities.ComplianceElementKind.TrainingRequirement"/> elements are missing.</param>
/// <param name="EventRoutingReady">True if every required program's event route (<see cref="FacilityEventRoutingRulesBuilder"/>) has a completed (non-missing) SOP.</param>
/// <param name="OverallReadinessPercent">The percentage of the three dimensions above that are ready.</param>
public sealed record RiskReadiness(
    Guid FacilityId,
    bool HazardResponseReady,
    bool EmergencyResponseReady,
    bool EventRoutingReady,
    double OverallReadinessPercent);

/// <summary>Builds <see cref="RiskReadiness"/> instances deterministically from a <see cref="FacilityProfile"/>.</summary>
public static class RiskReadinessBuilder
{
    /// <summary>Builds the risk readiness for the given facility.</summary>
    /// <exception cref="ArgumentException">The facility's <see cref="FacilityProfile.IndustryName"/> does not exist in <see cref="IndustryRegistry.All"/>.</exception>
    public static RiskReadiness Build(FacilityProfile facility)
    {
        IndustryDefinition industry = IndustryRegistry.FindByName(facility.IndustryName)
            ?? throw new ArgumentException($"No industry named '{facility.IndustryName}' exists in IndustryRegistry.All.", nameof(facility));

        var missingCodes = facility.ComplianceProfile.MissingElements
            .Select(element => element.Code)
            .ToHashSet(StringComparer.Ordinal);

        bool hazardResponseReady = HazardMapping.For(industry).Count == 0
            || facility.RequiredSops.All(sop => !missingCodes.Contains(sop.Code));

        bool emergencyResponseReady = !facility.ComplianceProfile.MissingElements
            .Any(element => element.Kind == ComplianceElementKind.TrainingRequirement);

        bool eventRoutingReady = FacilityEventRoutingRulesBuilder.Build(facility)
            .All(route => !missingCodes.Contains(route.SopCode));

        int readyCount = new[] { hazardResponseReady, emergencyResponseReady, eventRoutingReady }.Count(ready => ready);
        double overallReadinessPercent = readyCount / 3d * 100d;

        return new RiskReadiness(facility.FacilityId, hazardResponseReady, emergencyResponseReady, eventRoutingReady, overallReadinessPercent);
    }
}
