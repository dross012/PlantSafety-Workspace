// ============================================================================
//  Project:        Enterprise.Dashboard
//  Module:         Dtos
//  File:           EnterpriseSeverityDistributionDto.cs
//  Created:        2026-08-08
//  Description:    Phase D.4: a severity-level distribution for the
//                  dashboard. Same rationale as EnterpriseHazardDistributionDto
//                  -- the breakdown is caller-supplied since data lake
//                  payloads are opaque strings this project cannot classify.
// ============================================================================
namespace Enterprise.Dashboard.Dtos;

/// <summary>A severity-level distribution: count of records per severity label.</summary>
public sealed record EnterpriseSeverityDistributionDto(
    IReadOnlyDictionary<string, int> CountsBySeverityLabel,
    int TotalClassifiedRecords);
