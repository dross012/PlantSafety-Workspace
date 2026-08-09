// ============================================================================
//  Project:        Enterprise.Dashboard
//  Module:         Dtos
//  File:           EnterpriseComplianceSummaryDto.cs
//  Created:        2026-08-08
//  Description:    Phase D.4: a coarse, dashboard-facing compliance
//                  summary built purely from Enterprise.DataLake record
//                  counts -- no compliance percentage is invented here;
//                  see Shared.SafetyData's Phase C.10 SeverityAwareComplianceScore
//                  for the real, richer concept this deliberately does not
//                  duplicate.
// ============================================================================
namespace Enterprise.Dashboard.Dtos;

/// <summary>A coarse, dashboard-facing compliance summary derived from enterprise data lake record counts.</summary>
public sealed record EnterpriseComplianceSummaryDto(
    int TotalFacilities,
    int TotalEventRecords,
    DateTime GeneratedAtUtc);
