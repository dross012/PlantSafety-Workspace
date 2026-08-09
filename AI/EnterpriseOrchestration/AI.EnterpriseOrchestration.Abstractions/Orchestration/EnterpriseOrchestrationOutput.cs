// ============================================================================
//  Project:        AI.EnterpriseOrchestration
//  Module:         Abstractions.Orchestration
//  File:           EnterpriseOrchestrationOutput.cs
//  Created:        2026-08-08
//  Description:    Phase E.9: the result of an enterprise-orchestration
//                  session -- covers all five "across facilities"
//                  dimensions Path E's spec requires: workflows
//                  (FacilitiesRequiringEvacuation), hazard responses
//                  (HighestSeverityAcrossFacilities), regulatory
//                  compliance (FacilitiesWithRegulatoryExposure),
//                  predictive alerts (FacilitiesWithAnomalousPredictions),
//                  and digital twin simulations (the DigitalTwinSimulatedSeverity
//                  signal folded into HighestSeverityAcrossFacilities).
// ============================================================================
using Shared.FacilityDomain.Abstractions.Severity;

namespace AI.EnterpriseOrchestration.Abstractions.Orchestration;

/// <summary>The result of an enterprise-orchestration session.</summary>
public sealed record EnterpriseOrchestrationOutput(
    int TotalFacilities,
    IReadOnlyList<Guid> FacilitiesRequiringEvacuation,
    IReadOnlyList<Guid> FacilitiesWithRegulatoryExposure,
    IReadOnlyList<Guid> FacilitiesWithAnomalousPredictions,
    SeverityLevel HighestSeverityAcrossFacilities,
    IReadOnlyList<string> EnterpriseAlerts,
    DateTime GeneratedAtUtc);
