// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Application
//  File:           IRCADetailRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for a single RCA record by identity, backing
//                  GetRCAByIdQuery. Deliberately a new, distinct interface from the pre-existing
//                  PlantSafety.Application.ContinuousImprovement.RCA.Repositories.IRCARepository
//                  (the date-range list read-model behind GetRCAQuery/usp_RCA_GetRcaRecords),
//                  which this pass leaves entirely untouched -- it is a different, coarser
//                  reporting query, not the by-id path built here. Implemented in the
//                  Infrastructure layer by RCADetailDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.ContinuousImprovement.RCA.Models;

namespace PlantSafety.Application.ContinuousImprovement.RCA.Repositories;

/// <summary>Read-only repository for a single RCA record by identity. Implemented in the Infrastructure layer.</summary>
public interface IRCADetailRepository
{
    /// <summary>
    /// Returns the full RCA read-model record for the given identity, scoped to the given
    /// tenant, or <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<RCADetailDto?> GetByIdAsync(Guid rcaId, Guid tenantId, CancellationToken cancellationToken);
}
