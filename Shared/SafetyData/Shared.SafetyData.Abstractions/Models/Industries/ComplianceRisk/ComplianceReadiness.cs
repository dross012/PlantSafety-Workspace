// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.ComplianceRisk
//  Layer:          Abstractions
//  File:           ComplianceReadiness.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Readiness indicators for a facility's audits, training,
//                  permits, and SOPs -- computed strictly from FacilityProfile
//                  (its own ComplianceProfile.MissingElements, itself built
//                  from ProgramRequirementMapping via FacilityProfileBuilder)
//                  with no independent recomputation. A dimension is "ready"
//                  when no MissingElements of that ComplianceElementKind
//                  remain. Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

namespace Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;

/// <summary>
/// A single facility's readiness across its audit, training, permit, and SOP compliance
/// elements.
/// </summary>
/// <param name="FacilityId">The facility this readiness was computed for.</param>
/// <param name="AuditReady">True if no required <see cref="Facilities.ComplianceElementKind.AuditItem"/> elements are missing.</param>
/// <param name="TrainingReady">True if no required <see cref="Facilities.ComplianceElementKind.TrainingRequirement"/> elements are missing.</param>
/// <param name="PermitReady">True if no required <see cref="Facilities.ComplianceElementKind.Permit"/> elements are missing.</param>
/// <param name="SopReady">True if no required <see cref="Facilities.ComplianceElementKind.SopTemplate"/> elements are missing.</param>
/// <param name="OverallReadinessPercent">The percentage of the four dimensions above that are ready (0/25/50/75/100).</param>
public sealed record ComplianceReadiness(
    Guid FacilityId,
    bool AuditReady,
    bool TrainingReady,
    bool PermitReady,
    bool SopReady,
    double OverallReadinessPercent);

/// <summary>Builds <see cref="ComplianceReadiness"/> instances deterministically from a <see cref="FacilityProfile"/>.</summary>
public static class ComplianceReadinessBuilder
{
    /// <summary>Builds the compliance readiness for the given facility.</summary>
    public static ComplianceReadiness Build(FacilityProfile facility)
    {
        var missingKinds = facility.ComplianceProfile.MissingElements
            .Select(element => element.Kind)
            .ToHashSet();

        bool auditReady = !missingKinds.Contains(ComplianceElementKind.AuditItem);
        bool trainingReady = !missingKinds.Contains(ComplianceElementKind.TrainingRequirement);
        bool permitReady = !missingKinds.Contains(ComplianceElementKind.Permit);
        bool sopReady = !missingKinds.Contains(ComplianceElementKind.SopTemplate);

        int readyCount = new[] { auditReady, trainingReady, permitReady, sopReady }.Count(ready => ready);
        double overallReadinessPercent = readyCount / 4d * 100d;

        return new ComplianceReadiness(facility.FacilityId, auditReady, trainingReady, permitReady, sopReady, overallReadinessPercent);
    }
}
