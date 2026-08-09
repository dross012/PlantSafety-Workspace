// ============================================================================
//  Project:        Enterprise.Dashboard
//  Module:         Dtos
//  File:           EnterpriseRiskSummaryDto.cs
//  Created:        2026-08-08
//  Description:    Phase D.4 (Path D: Enterprise-Scale Safety Platform
//                  Modernization): a coarse, dashboard-facing risk summary
//                  built purely from Enterprise.DataLake record counts
//                  (Phase D.2) -- no risk score is invented here; a real
//                  numeric risk score (see Shared.SafetyData's Phase C.10
//                  SeverityAwareRiskScore) is a separate, richer concept
//                  this DTO deliberately does not duplicate or approximate.
// ============================================================================
namespace Enterprise.Dashboard.Dtos;

/// <summary>A coarse, dashboard-facing risk summary derived from enterprise data lake record counts.</summary>
public sealed record EnterpriseRiskSummaryDto(
    int TotalFacilities,
    int TotalHazardRecords,
    DateTime GeneratedAtUtc);
