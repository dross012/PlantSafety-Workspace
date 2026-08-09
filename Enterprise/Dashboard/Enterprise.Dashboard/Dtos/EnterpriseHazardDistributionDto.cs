// ============================================================================
//  Project:        Enterprise.Dashboard
//  Module:         Dtos
//  File:           EnterpriseHazardDistributionDto.cs
//  Created:        2026-08-08
//  Description:    Phase D.4: a hazard-type distribution for the
//                  dashboard. The breakdown itself is supplied by the
//                  caller (whoever has structured HazardType data, e.g. a
//                  real host with access to Shared.FacilityDomain) --
//                  Enterprise.DataLake's record payloads are opaque
//                  caller-serialized strings (Phase D.2), so this project
//                  cannot classify them itself without guessing.
// ============================================================================
namespace Enterprise.Dashboard.Dtos;

/// <summary>A hazard-type distribution: count of records per hazard label.</summary>
public sealed record EnterpriseHazardDistributionDto(
    IReadOnlyDictionary<string, int> CountsByHazardLabel,
    int TotalHazardRecords);
