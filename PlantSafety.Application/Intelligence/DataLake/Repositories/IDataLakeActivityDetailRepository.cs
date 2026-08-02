// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.DataLake
//  Layer:          Application
//  File:           IDataLakeActivityDetailRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for a single DataLakeActivity record by
//                  identity, backing GetDataLakeActivityByIdQuery. Deliberately a new, distinct
//                  interface from the pre-existing
//                  PlantSafety.Application.Intelligence.DataLake.Repositories.IDataLakeRepository
//                  (the date-range list read-model behind
//                  GetDataLakeQuery/usp_DataLake_GetDataLakeRecords), which this pass leaves
//                  entirely untouched. Implemented in the Infrastructure layer by
//                  DataLakeActivityDetailDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.DataLake.Models;

namespace PlantSafety.Application.Intelligence.DataLake.Repositories;

/// <summary>Read-only repository for a single DataLakeActivity record by identity. Implemented in the Infrastructure layer.</summary>
public interface IDataLakeActivityDetailRepository
{
    /// <summary>
    /// Returns the full DataLakeActivity read-model record for the given identity, scoped to the
    /// given tenant, or <see langword="null"/> if not found or if it belongs to a different tenant.
    /// </summary>
    Task<DataLakeActivityDetailDto?> GetByIdAsync(Guid dataLakeActivityId, Guid tenantId, CancellationToken cancellationToken);
}
