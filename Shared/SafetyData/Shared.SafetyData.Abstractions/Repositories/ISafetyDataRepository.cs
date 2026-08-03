// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Repositories
//  Layer:          Abstractions
//  File:           ISafetyDataRepository.cs
//  Created:        2026-07-27
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction over PlantSafety's real
//                  Hazard/Risk, Corrective Action, and derived Safety
//                  Compliance data -- [PlantSafety.CoreDB].[RiskMgmt].[Risk],
//                  [RiskMgmt].[ControlMeasure], and
//                  [ContinuousImprovement].[CAPA]. Implemented in
//                  Shared.SafetyData.Infrastructure against PlantSafety.CoreDB
//                  directly.
//
//                  This module exists so Reporting can query real PlantSafety
//                  reporting data (Hazard Summary, Corrective Action, Safety
//                  Compliance report types) without taking a direct project
//                  dependency on PlantSafety -- the exact pattern
//                  Shared.AuditLogging already established for dbo.AuditLog.
//                  See this module's docs\README.md.
//
//                  SAFETY: This repository is deliberately READ-ONLY. It may
//                  never INSERT, UPDATE, or DELETE against PlantSafety's
//                  tenant-owned tables -- those are mutated exclusively through
//                  PlantSafety's own aggregates/repositories/stored procedures.
//                  Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models;

namespace Shared.SafetyData.Abstractions.Repositories;

/// <summary>
/// Read-only repository over PlantSafety's Hazard/Risk, Corrective Action, and derived
/// Safety Compliance data.
/// </summary>
public interface ISafetyDataRepository
{
    /// <summary>
    /// Retrieves a page of hazard/risk records ([RiskMgmt].[Risk], joined to
    /// [RiskMgmt].[ControlMeasure]) for the given tenant, newest first, via
    /// dbo.usp_GetHazardSummary. Never mutates PlantSafety data -- SELECT only.
    /// </summary>
    Task<SafetySearchResult<HazardSummaryEntry>> GetHazardSummaryAsync(SafetySearchCriteria criteria, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a page of corrective action records ([ContinuousImprovement].[CAPA])
    /// for the given tenant, newest first, via dbo.usp_GetCorrectiveActions. Never
    /// mutates PlantSafety data -- SELECT only.
    /// </summary>
    Task<SafetySearchResult<CorrectiveActionEntry>> GetCorrectiveActionsAsync(SafetySearchCriteria criteria, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the derived open/closed/overdue safety-compliance rollup (one row per
    /// category -- "Hazards" and "CorrectiveActions") for the given tenant, via
    /// dbo.usp_GetSafetyCompliance. Not paginated -- PageNumber/PageSize on
    /// <paramref name="criteria"/> are ignored. Never mutates PlantSafety data -- SELECT only.
    /// </summary>
    Task<IReadOnlyList<SafetyComplianceEntry>> GetSafetyComplianceAsync(SafetySearchCriteria criteria, CancellationToken cancellationToken);
}
