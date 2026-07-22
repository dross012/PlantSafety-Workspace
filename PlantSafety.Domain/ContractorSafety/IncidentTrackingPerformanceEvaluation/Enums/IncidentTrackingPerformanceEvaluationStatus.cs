// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           IncidentTrackingPerformanceEvaluationStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real investigation/eligibility lifecycle states for the
//                  IncidentTrackingPerformanceEvaluation aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Enums;

/// <summary>Lifecycle states for the <see cref="IncidentTrackingPerformanceEvaluation"/> aggregate root.</summary>
public enum IncidentTrackingPerformanceEvaluationStatus
{
    /// <summary>Incident/near-miss/unsafe condition reported for a contractor.</summary>
    Reported = 0,

    /// <summary>Investigation and root-cause analysis in progress.</summary>
    InvestigationInProgress = 1,

    /// <summary>Corrective and preventive actions (CAPA) documented.</summary>
    CapaDocumented = 2,

    /// <summary>Contractor deemed eligible for future work based on safety performance.</summary>
    EligibilityApproved = 3,

    /// <summary>Contractor's eligibility for future work restricted pending improvement.</summary>
    EligibilityRestricted = 4,
}
